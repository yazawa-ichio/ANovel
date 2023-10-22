namespace ANovel.Core
{

	public class LineReader
	{
		string m_Name;
		int m_Index;
		string[] m_Line;

		public int Index => m_Index;

		public string Path => m_Name;

		public bool EndOfFile => m_Index > m_Line.Length;

		public bool InsertLastStop { get; set; } = true;

		public LineReader(string name, string text)
		{
			m_Name = name;
			m_Line = text.Split(Token.NewLine, System.StringSplitOptions.None);
		}

		public void Reset()
		{
			m_Index = 0;
		}

		public void SeekIndex(int index)
		{
			m_Index = index;
		}

		public void SeekLabel(string label)
		{
			m_Index = 0;
			LineData data = default;
			while (TryRead(ref data))
			{
				if (data.Type == LineType.Label && (data.ReadName(out _) == label))
				{
					return;
				}
			}
			throw new System.Exception($"not found label:{label} {m_Name}");
		}

		public bool TryRead(ref LineData data, bool skipNoneAndComment = true)
		{
			while (m_Index < m_Line.Length)
			{
				var index = m_Index++;
				var line = m_Line[index];
				var type = GetType(line);
				if (!skipNoneAndComment || (type != LineType.None && type != LineType.Comment))
				{
					data = new LineData(m_Name, line, index, type);
					return true;
				}
			}
			if (m_Index == m_Line.Length)
			{
				if (!InsertLastStop)
				{
					return false;
				}
				var index = m_Index++;
				data = new LineData("stop", "&stop", index, LineType.SystemCommand);
				return true;
			}
			return false;
		}

		public bool TryPeekType(out LineType type)
		{
			if (m_Index < m_Line.Length)
			{
				var line = m_Line[m_Index];
				type = GetType(line);
				return true;
			}
			type = LineType.None;
			return false;
		}

		LineType GetType(string line)
		{
			for (int i = 0; i < line.Length; i++)
			{
				var c = line[i];
				if (Token.IsEmptyOrTab(c))
				{
					continue;
				}
				switch (c)
				{
					case Token.Comment:
						return LineType.Comment;
					case Token.PreProcess:
						return LineType.PreProcess;
					case Token.Command:
						return LineType.Command;
					case Token.SystemCommand:
						return LineType.SystemCommand;
					case Token.Label:
						return LineType.Label;
					default:
						return LineType.Text;
				}
			}
			return LineType.None;
		}

	}
}