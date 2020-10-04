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

		public string Get()
		{
			return Get("\n");
		}

		public string Get(string newline)
		{
			m_StringBuilder.Clear();
			for (int i = 0; i < m_Lines.Count; i++)
			{
				var line = m_Lines[i].Line;
				m_StringBuilder.Append(line);
				if (i + 1 < m_Lines.Count)
				{
					m_StringBuilder.Append(newline);
				}
			}
			return m_StringBuilder.ToString();
		}

	}
}