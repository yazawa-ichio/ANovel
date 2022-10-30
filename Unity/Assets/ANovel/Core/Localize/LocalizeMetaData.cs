using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{

	public class LocalizeMetaData
	{
		LocalizeData m_Data;
		bool m_UseKey;
		LanguageData m_Default;

		public bool UseKey => m_UseKey;

		public LocalizeMetaData(LocalizeData data)
		{
			m_Data = data;
			m_Default = m_Data.List.First(x => x.Default);
		}

		public LocalizeMetaData(LocalizeData data, bool useKey) : this(data)
		{
			m_UseKey = useKey;
		}

		public int GetKeyIndex(string key)
		{
			return Array.IndexOf(m_Data.Keys, key);
		}

		public IEnumerable<LocalizeTextEnvData> GetTexts(int index)
		{
			foreach (var data in m_Data.List)
			{
				var text = data.Lines[index];
				yield return new LocalizeTextEnvData
				{
					Default = data.Default,
					Lang = data.Language,
					Text = text,
				};
			}
		}

		public IEnumerable<LocalizeTextEnvData> GetTexts(string key)
		{
			int index;
			if (m_Data.Keys != null && m_Data.Keys.Length > 0)
			{
				index = Array.IndexOf(m_Data.Keys, key);
			}
			else
			{
				index = Array.IndexOf(m_Default.Lines, key);
			}
			return GetTexts(index);
		}


	}
}