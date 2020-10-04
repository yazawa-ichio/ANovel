using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public class BlockReader
	{
		IFileLoader m_Loader;
		LineReader m_LineReader;
		PreProcessor m_PreProcessor;
		TagProvider m_TagProvider;
		PreProcessor.Result m_PreProcess;
		List<Tag> m_Tags = new List<Tag>();
		List<LineData> m_Text = new List<LineData>();
		LabelData m_Label = new LabelData();

		public bool EndOfFile => m_LineReader?.EndOfFile ?? false;

		public BlockReader(IFileLoader loader) : this(loader, Array.Empty<string>())
		{
		}

		public BlockReader(IFileLoader loader, string[] symbols)
		{
			m_Loader = loader;
			m_PreProcessor = new PreProcessor(loader, symbols);
			m_TagProvider = new TagProvider();
			if (symbols != null)
			{
				m_TagProvider.Symbols.AddRange(symbols);
			}
		}

		public async Task Load(string path)
		{
			m_Label.Reset();
			var text = await m_Loader.Load(path);
			m_LineReader = new LineReader(path, text);
			m_PreProcess = await m_PreProcessor.Run(path, text);
			m_TagProvider.Setup(m_PreProcess);
		}

		public bool TryRead(Block block)
		{
			block.Clear();
			if (EndOfFile)
			{
				return false;
			}
			bool first = true;
			LineData data = default;
			while (TryReadLine(ref data, skipNoneAndComment: true))
			{
				switch (data.Type)
				{
					case LineType.Command:
					case LineType.SystemCommand:
						OnCommand(in data, block.Commands);
						break;
					case LineType.Label:
						OnLabel(in data, first);
						break;
				}
				if (first)
				{
					first = false;
					block.LabelInfo = m_Label.GetInfo(in data);
				}
				if (data.Type == LineType.Text)
				{
					OnText(in data, block.Text);
					return true;
				}
			}
			return !first;
		}

		void OnCommand(in LineData data, List<Command> list)
		{
			m_Tags.Clear();
			m_TagProvider.Provide(in data, m_Tags);
			foreach (var tag in m_Tags)
			{
				list.Add(tag as Command);
			}
		}

		void OnLabel(in LineData data, bool first)
		{
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