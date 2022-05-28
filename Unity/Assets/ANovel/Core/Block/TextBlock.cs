using ANovel.Core;
using ANovel.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ANovel
{
	public enum TextBlockType
	{
		Text,
		Extension,
	}

	public class TextBlock : IDisposable, ICustomMapSerialization
	{
		static ThreadLocal<StringBuilder> s_StringBuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder());

		List<string> m_Lines;

		public TextBlockType Type { get; private set; }

		public ExtensionTextBlockInfo Extension { get; private set; }

		public int LineCount => m_Lines.Count;

		int ICustomMapSerialization.Length => Type == TextBlockType.Text ? 1 : 2;

		public TextBlock()
		{
			m_Lines = ListPool<string>.Pop();
		}

		public void Dispose()
		{
			if (m_Lines != null)
			{
				ListPool<string>.Push(m_Lines);
				m_Lines = null;
			}
		}

		public TextBlock Clone()
		{
			var block = new TextBlock
			{
				Type = Type,
				Extension = Extension
			};
			foreach (var line in m_Lines)
			{
				block.m_Lines.Add(line);
			}
			return block;
		}

		public void SetTexts(List<LineData> texts)
		{
			Type = TextBlockType.Text;
			m_Lines.Clear();
			foreach (var text in texts)
			{
				m_Lines.Add(text.Line);
			}
		}

		public void SetTexts(in ExtensionTextBlockInfo info, List<LineData> texts)
		{
			Extension = info;
			Type = TextBlockType.Extension;
			m_Lines.Clear();
			foreach (var text in texts)
			{
				m_Lines.Add(text.Line);
			}
		}

		public string GetLine(int index)
		{
			return m_Lines[index];
		}

		public string Get()
		{
			return Get("\n");
		}

		public string Get(string newline)
		{
			return GetImpl(newline, 0, -1);
		}

		public string GetRange(int start, int count)
		{
			return GetImpl("\n", start, count);
		}

		public string GetRange(int start)
		{
			return GetImpl("\n", start, -1);
		}

		string GetImpl(string newline, int start, int count)
		{
			var sb = s_StringBuilder.Value;
			try
			{
				sb.Clear();
				if (count < 0)
				{
					count = m_Lines.Count - start;
				}
				for (int i = 0; i < count; i++)
				{
					var lineIndex = start + i;
					var line = m_Lines[lineIndex];
					sb.Append(line);
					if (i + 1 < count)
					{
						sb.Append(newline);
					}
				}
				return sb.ToString();
			}
			finally
			{
				sb.Clear();
			}
		}

		void ICustomMapSerialization.Write(Writer writer)
		{
			if (Type == TextBlockType.Extension)
			{
				writer.Write("Extension");
				writer.WriteMapHeader(2);
				writer.Write("Name");
				writer.Write(Extension.Name);
				writer.Write("Value");
				writer.Write(Extension.Value);
			}
			writer.Write("Lines");
			writer.WriteArrayHeader(m_Lines.Count);
			for (int i = 0; i < m_Lines.Count; i++)
			{
				writer.Write(m_Lines[i]);
			}
		}

		void ICustomMapSerialization.Read(int length, Reader reader)
		{
			Type = TextBlockType.Text;
			m_Lines.Clear();
			for (int i = 0; i < length; i++)
			{
				switch (reader.ReadString())
				{
					case "Extension":
						ReadExtension(reader);
						break;
					case "Lines":
						ReadLines(reader);
						break;
				}

			}
		}

		void ReadExtension(Reader reader)
		{
			string name = default;
			string value = default;
			var length = reader.ReadMap();
			for (int i = 0; i < length; i++)
			{
				switch (reader.ReadString())
				{
					case "Name":
						name = reader.ReadString();
						break;
					case "Value":
						value = reader.ReadString();
						break;
				}
			}
			Type = TextBlockType.Extension;
			Extension = new ExtensionTextBlockInfo(name, value);
		}

		void ReadLines(Reader reader)
		{
			m_Lines.Clear();
			var len = reader.ReadArray();
			for (int j = 0; j < len; j++)
			{
				m_Lines.Add(reader.ReadString());
			}
		}

	}
}