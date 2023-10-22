using System.Collections.Generic;

namespace ANovel.Core
{
	[System.Serializable]
	public class LineTextData
	{
		public string Value;
	}

	[System.Serializable]
	public class LanguageData
	{
		public bool Default;
		public string Language;
		public string[] Lines;
	}

	public class KeyIndexData
	{
		public int Index;
		public string Key;
	}

	[System.Serializable]
	public class LocalizeData
	{
		public string[] Keys;
		public List<LanguageData> List = new(4);

		public static LocalizeData Create(string text, bool tab)
		{
			var data = new LocalizeData();
			var parseData = CsvParser.LoadFromString(text, tab ? CsvParser.Delimiter.Tab : CsvParser.Delimiter.Comma);
			var first = parseData[0];
			int? keyIndex = null;
			Dictionary<int, LanguageData> dic = new();
			for (int i = 0; i < first.Count; i++)
			{
				var langKey = first[i];
				if (string.IsNullOrEmpty(langKey) || langKey.ToLower() == "key")
				{
					if (keyIndex.HasValue)
					{
						continue;
					}
					keyIndex = i;
					data.Keys = new string[parseData.Count - 1];
				}
				else
				{
					var lang = new LanguageData();
					lang.Default = data.List.Count == 0;
					lang.Language = langKey;
					lang.Lines = new string[parseData.Count - 1];
					data.List.Add(lang);
					dic[i] = lang;
				}
			}
			for (int i = 1; i < parseData.Count; i++)
			{
				var lines = parseData[i];
				for (int langIndex = 0; langIndex < lines.Count; langIndex++)
				{
					var value = lines[langIndex];
					if (keyIndex == langIndex)
					{
						data.Keys[i - 1] = value;
					}
					else if (dic.TryGetValue(langIndex, out var lang))
					{
						lang.Lines[i - 1] = value;
					}
				}
			}
			return data;
		}
	}


}