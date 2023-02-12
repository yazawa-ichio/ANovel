using System.Text;

namespace ANovel.Core
{
	public class LocalizedText : ITextBlock
	{
		static StringBuilder s_StringBuilder = new StringBuilder();

		string[] m_Lines;

		public int LineCount => m_Lines.Length;

		public LocalizedText(string text)
		{
			m_Lines = text.Split(Token.NewLine, System.StringSplitOptions.None);
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
			var sb = s_StringBuilder.Clear();
			try
			{
				if (count < 0)
				{
					count = m_Lines.Length - start;
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
	}
}