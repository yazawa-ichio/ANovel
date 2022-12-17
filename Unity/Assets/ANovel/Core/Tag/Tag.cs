using System.Collections.Generic;

namespace ANovel.Core
{
	public abstract class Tag
	{
		public string TagName { get; private set; }

		public LineData LineData { get; private set; }

		IReadOnlyDictionary<string, string> m_Dic;

		public IReadOnlyDictionary<string, string> Dic => m_Dic;

		internal void Set(string name, in LineData data, IReadOnlyDictionary<string, string> dic)
		{
			TagName = name;
			LineData = data;
			m_Dic = dic;
		}

	}
}