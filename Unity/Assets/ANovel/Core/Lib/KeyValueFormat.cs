using System.Collections.Generic;
using System.Text;

namespace ANovel.Core
{
	public class KeyValueFormat
	{
		string m_Path;
		object[] m_Args;
		List<string> m_Keys = new List<string>();

		public KeyValueFormat(string path)
		{
			Parse(path);
			m_Args = new object[m_Keys.Count];
		}

		public string Format(Dictionary<string, string> dic)
		{
			try
			{
				for (int i = 0; i < m_Keys.Count; i++)
				{
					m_Args[i] = dic[m_Keys[i]];
				}
				return string.Format(m_Path, m_Args);
			}
			finally
			{
				System.Array.Clear(m_Args, 0, m_Args.Length);
			}
		}

		public string Convert(Dictionary<string, object> dic)
		{
			try
			{
				for (int i = 0; i < m_Keys.Count; i++)
				{
					if (dic.TryGetValue(m_Keys[i], out var obj))
					{
						m_Args[i] = obj;
					}
				}
				return string.Format(m_Path, m_Args);
			}
			finally
			{
				System.Array.Clear(m_Args, 0, m_Args.Length);
			}
		}

		void Parse(string path)
		{
			var body = new StringBuilder();
			for (int i = 0; i < path.Length; i++)
			{
				char c = path[i];
				if (c == '{')
				{
					body.Append(c);
					i++;
					var key = ParseKey(ref i, path);
					body.Append(key);
				}
				else
				{
					body.Append(c);
				}
			}
			m_Path = body.ToString();
		}

		string ParseKey(ref int i, string path)
		{
			var suffix = false;
			var key = new StringBuilder();
			var result = new StringBuilder();
			result.Append(m_Keys.Count);
			while (i < path.Length)
			{
				var c = path[i];
				if (c == '}')
				{
					result.Append(c);
					break;
				}
				if (c == ':' || suffix)
				{
					suffix = true;
					result.Append(c);
					i++;
					continue;
				}
				key.Append(c);
				i++;
			}
			m_Keys.Add(key.ToString());
			return result.ToString();
		}

	}

}