using System.Collections.Generic;
using System.Text;

namespace ANovel.Core
{
	public enum TextBlockType
	{
		Text,
		Extension,
	}

	public class TextBlock
	{

		List<LineData> m_Lines = new List<LineData>();
		StringBuilder m_StringBuilder = new StringBuilder();

		public TextBlockType Type { get; private set; }

		public ExtensionTextBlockInfo Extension { get; private set; }

		public int LineCount => m_Lines.Count;

		public void Clear()
		{
			m_Lines.Clear();
			Type = TextBlockType.Text;
			Extension = default;
		}

		public void SetTexts(List<LineData> texts)
		{
			Type = TextBlockType.Text;
			m_Lines.Clear();
			m_Lines.AddRange(texts);
		}

		public void SetTexts(in ExtensionTextBlockInfo info, List<LineData> texts)
		{
			Extension = info;
			Type = TextBlockType.Extension;
			m_Lines.Clear();
			m_Lines.AddRange(texts);
		}

		public string GetLine(int index)
		{
			return m_Lines[index].Line;
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
			m_StringBuilder.Clear();
			if (count < 0)
			{
				count = m_Lines.Count - start;
			}
			for (int i = 0; i < count; i++)
			{
				var lineIndex = start + i;
				var line = m_Lines[lineIndex].Line;
				m_StringBuilder.Append(line);
				if (lineIndex + 1 < count)
				{
					m_StringBuilder.Append(newline);
				}
			}
			return m_StringBuilder.ToString();
		}

	}
}