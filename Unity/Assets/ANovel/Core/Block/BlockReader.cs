using ANovel.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public class BlockReader
	{
		IScenarioLoader m_Loader;
		LineReader m_LineReader;
		PreProcessor m_PreProcessor;
		TagProvider m_TagProvider;
		PreProcessResult m_PreProcess;
		List<Tag> m_Tags = new List<Tag>();
		List<LineData> m_Text = new List<LineData>();
		List<ICommand> m_Commands = new List<ICommand>();
		LabelData m_Label = new LabelData();
		bool m_Stop;
		Evaluator m_Evaluator = new Evaluator();

		public Evaluator Evaluator => m_Evaluator;

		public int LineIndex => m_LineReader.Index;

		public bool CanRead => !m_Stop && m_LineReader != null && !m_LineReader.EndOfFile;

		public BranchController BranchController => m_TagProvider.BranchController;

		public BlockReader(IScenarioLoader loader, string[] symbols)
		{
			m_Loader = loader;
			m_Evaluator = new Evaluator();
			m_PreProcessor = new PreProcessor(loader, m_Evaluator, symbols);
			var symbolsList = new List<string>();
			if (symbols != null)
			{
				symbolsList.AddRange(symbols);
			}
			m_TagProvider = new TagProvider(m_Evaluator, symbolsList);
		}

		public async Task<PreProcessResult> Load(string path, CancellationToken token)
		{
			m_Stop = false;
			m_Label.Reset();
			var text = await m_Loader.Load(path, token);
			m_LineReader = new LineReader(path, text);
			m_PreProcess = await m_PreProcessor.Run(path, text, token);
			m_TagProvider.Setup(m_PreProcess);
			return m_PreProcess;
		}

		public bool TryRead(out Block block)
		{
			block = null;
			if (!CanRead)
			{
				return false;
			}
			bool firstRead = true;
			BlockLabelInfo label = default;
			TextBlock text = null;
			m_Commands.Clear();
			LineData data = default;
			while (TryReadLine(ref data, skipNoneAndComment: true))
			{
				bool endBlock = false;
				switch (data.Type)
				{
					case LineType.Command:
					case LineType.SystemCommand:
						OnCommand(in data, m_Commands, out endBlock);
						break;
					case LineType.Label:
						OnLabel(in data, firstRead);
						break;
				}
				if (firstRead)
				{
					firstRead = false;
					label = m_Label.GetInfo(in data);
				}
				if (endBlock)
				{
					break;
				}
				if (m_TagProvider.BranchController.IsSkip)
				{
					continue;
				}
				if (data.Type == LineType.Text)
				{
					if (text == null) text = new TextBlock();
					OnText(in data, text);
					break;
				}
			}
			if (!firstRead)
			{
				block = new Block(m_LineReader.Path, in label, m_Commands, text, m_PreProcess.Meta);
				m_Stop = block.StopCommand != null;
			}
			m_Commands.Clear();
			return !firstRead;
		}

		public void Seek(BlockLabelInfo info)
		{
			m_Stop = false;
			if (info.LineIndex >= 0)
			{
				m_LineReader.SeekIndex(info.LineIndex);
			}
			else
			{
				if (!string.IsNullOrEmpty(info.Name))
				{
					m_LineReader.SeekLabel(info.Name);
				}
				else
				{
					m_LineReader.SeekIndex(0);
				}
				for (int i = 0; i < info.BlockIndex; i++)
				{
					if (!TryRead(out _))
					{
						throw new System.Exception($"not found label:{info.Name}:{info.BlockIndex}");
					}
				}
			}
		}

		void OnCommand(in LineData data, List<ICommand> list, out bool endBlock)
		{
			endBlock = false;
			m_Tags.Clear();
			m_TagProvider.Provide(in data, m_Tags);
			foreach (var tag in m_Tags)
			{
				list.Add((ICommand)tag);
				if (tag is IEndBlockCommand)
				{
					endBlock = true;
				}
			}
		}

		void OnLabel(in LineData data, bool first)
		{
			if (m_TagProvider.BranchController.IsIfScope)
			{
				throw new LineDataException(in data, "label is not allow if scope");
			}
			if (!first)
			{
				throw new LineDataException(in data, "label is text block top only");
			}
			m_Label.Set(in data, m_TagProvider.Converters);
		}

		void OnText(in LineData start, TextBlock block)
		{
			m_Text.Clear();
			if (start.Line.StartsWith(Token.TextBlockScope, StringComparison.Ordinal))
			{
				bool extensionName = ExtensionTextBlockInfo.TryGet(in start, out var info);
				bool end = false;
				LineData data = default;
				while (TryReadLine(ref data, skipNoneAndComment: false))
				{
					if (data.Type == LineType.Text && data.Line == Token.TextBlockScope)
					{
						end = true;
						break;
					}
					m_Text.Add(data);
				}

				if (!end) throw new LineDataException(in start, "text block scope not close (```)");

				if (extensionName)
				{
					block.SetTexts(in info, m_Text);
				}
				else
				{
					block.SetTexts(m_Text);
				}

			}
			else
			{
				m_Text.Add(start);
				LineData data = default;
				while (m_LineReader.TryPeekType(out var type) && type == LineType.Text)
				{
					//事前にテキストがあると分かっているので問題ない
					TryReadLine(ref data, skipNoneAndComment: false);
					m_Text.Add(data);
				}

				block.SetTexts(m_Text);
			}
		}

		bool TryReadLine(ref LineData data, bool skipNoneAndComment)
		{
			while (m_LineReader.TryRead(ref data, skipNoneAndComment))
			{
				if (data.Type == LineType.PreProcess)
				{
					continue;
				}
				bool skip = false;
				foreach (var scope in m_PreProcess.SkipScopes)
				{
					if (scope.IsSkip(data.Index))
					{
						skip = true;
						break;
					}
				}
				if (!skip)
				{
					return true;
				}
			}
			return false;
		}

	}

}