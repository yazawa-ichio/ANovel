using ANovel.Core;
using ANovel.GoogleApis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ANovel.Engine.Tools
{
	public static class LocalizeSpreadsheetSync
	{
		public static async void Push(string assetPath, string sheetId, string sheetName)
		{
			try
			{
				if (string.IsNullOrEmpty(sheetName))
				{
					sheetName = Path.GetFileNameWithoutExtension(assetPath);
					if (sheetName.EndsWith(".lang"))
					{
						sheetName = sheetName.Substring(0, sheetName.Length - ".lang".Length);
					}
				}
				EditorUtility.DisplayProgressBar("Push Sheet", "get sheet list", 0);
				var data = JsonUtility.FromJson<LocalizeData>(File.ReadAllText(assetPath));
				var client = new SpreadSheetClient();
				var sheets = await client.Get(sheetId);
				if (!sheets.Sheets.Any(x => x.Properties.Title == sheetName))
				{
					EditorUtility.DisplayProgressBar("Push Sheet", "add sheet", 0.5f);
					// TODO:プロパティでレイアウトをつける
					await client.AddSheet(sheetId, sheetName);
				}

				var start = SpreadSheetUtil.ColumnIndexToName(1);
				var end = SpreadSheetUtil.ColumnIndexToName(data.List.Count + 1);
				var range = $"{sheetName}!{start}1:{end}{data.Keys.Length + 1}";

				EditorUtility.DisplayProgressBar("Push Sheet", "get sheet info", 0.75f);
				var sheet = await client.GetValues(sheetId, range);

				var valueData = new ValueRange()
				{
					Range = range,
					MajorDimension = "ROWS",
				};

				var list = new List<List<string>>();
				{
					var row = new List<string>(data.List.Count + 1);
					row.Add("key");
					for (int j = 0; j < data.List.Count; j++)
					{
						var lang = data.List[j];
						row.Add(lang.Language);
					}
					list.Add(row);
				}
				for (int i = 0; i < data.Keys.Length; i++)
				{
					var row = new List<string>(data.List.Count + 1);
					row.Add(data.Keys[i]);
					for (int j = 0; j < data.List.Count; j++)
					{
						var lang = data.List[j];
						row.Add(Get(data.Keys[i], j + 1, lang.Lines[i]));
					}
					list.Add(row);
				}
				valueData.Values = list;

				EditorUtility.DisplayProgressBar("Push Sheet", "update sheet", 0.9f);
				await client.Update(sheetId, valueData);
				EditorUtility.DisplayProgressBar("Push Sheet", "update sheet complete", 1f);

				string Get(string key, int rowIndex, string current)
				{
					foreach (var row in sheet.Values)
					{
						if (row.Count <= rowIndex)
						{
							continue;
						}
						if (row[0] == key)
						{
							if (string.IsNullOrEmpty(row[rowIndex]))
							{
								return current;
							}
							return row[rowIndex];
						}
					}
					return current;
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

		}

		public static async void Pull(string assetPath, string sheetId, string sheetName)
		{
			try
			{
				if (string.IsNullOrEmpty(sheetName))
				{
					sheetName = Path.GetFileNameWithoutExtension(assetPath);
					if (sheetName.EndsWith(".lang"))
					{
						sheetName = sheetName.Substring(0, sheetName.Length - ".lang".Length);
					}
				}

				var langCount = ANovelEditorConfig.Instance.Localize.Language.Count;

				var start = SpreadSheetUtil.ColumnIndexToName(1);
				var end = SpreadSheetUtil.ColumnIndexToName(langCount + 1);
				var range = $"{sheetName}!{start}1:{end}100000";

				var client = new SpreadSheetClient();

				EditorUtility.DisplayProgressBar("Pull Sheet", "update sheet", 0.5f);
				var values = await client.GetValues(sheetId, range);

				if (values.Values.Count == 0)
				{
					throw new System.Exception("sheet is empty");
				}

				var data = new LocalizeData();

				List<string> keys = new();
				Dictionary<int, (LanguageData, List<string>)> dic = new();

				for (int i = 0; i < values.Values[0].Count; i++)
				{
					var row = values.Values[0];
					string header = row[i];
					if (header == "key")
					{
						continue;
					}
					var langData = new LanguageData()
					{
						Default = dic.Count == 0,
						Language = header,
					};
					dic[i] = (langData, new List<string>());
				}
				for (int i = 1; i < values.Values.Count; i++)
				{
					var row = values.Values[i];
					if (row.Count == 0 || string.IsNullOrEmpty(row[0]))
					{
						continue;
					}
					keys.Add(row[0]);
					foreach (var kvp in dic)
					{
						var index = kvp.Key;
						if (row.Count <= index)
						{
							kvp.Value.Item2.Add("");
						}
						else
						{
							kvp.Value.Item2.Add(row[index]);
						}
					}
				}
				data.Keys = keys.ToArray();
				foreach ((LanguageData lang, List<string> list) in dic.Values)
				{
					lang.Lines = list.ToArray();
					data.List.Add(lang);
				}
				var json = JsonUtility.ToJson(data, true);
				File.WriteAllText(assetPath, json);
				AssetDatabase.Refresh();

			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}
	}
}