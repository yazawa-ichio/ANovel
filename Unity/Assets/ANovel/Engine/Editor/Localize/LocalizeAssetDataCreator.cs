using ANovel.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ANovel.Engine.Tools
{
	public class LocalizeAssetDataCreator : ScriptableWizard
	{
		public string SourcePath;

		protected override bool DrawWizardGUI()
		{
			var ret = base.DrawWizardGUI();
			return ret;
		}

		public void OnWizardCreate()
		{
			if (!File.Exists(SourcePath))
			{
				Debug.LogError("File not found");
				return;
			}
			if (Path.GetExtension(SourcePath) != ".anovel")
			{
				return;
			}
			var converter = new LocalizeDefineConverter(File.ReadAllText(SourcePath));

			var langPath = Path.ChangeExtension(SourcePath, "lang.anovellang");
			if (File.Exists(langPath))
			{
				var langData = File.ReadAllText(langPath);
				LocalizeData current;
				if (langData.StartsWith("{"))
				{
					current = JsonUtility.FromJson<LocalizeData>(langData);
				}
				else
				{
					current = LocalizeData.Create(langData, tab: false);
				}

				LocalizeData data = new();
				data.Keys = converter.KeyAndValues.Keys.ToArray();

				foreach (var oldLang in current.List)
				{
					var newLang = new LanguageData();
					newLang.Default = oldLang.Default;
					newLang.Language = oldLang.Language;

					List<string> lines = new();
					foreach (var key in data.Keys)
					{
						var index = Array.IndexOf(current.Keys, key);
						if (index >= 0)
						{
							lines.Add(oldLang.Lines[index]);
						}
						else
						{
							lines.Add(converter.KeyAndValues[key]);
						}
					}
					newLang.Lines = lines.ToArray();
					data.List.Add(newLang);

				}
				var json = JsonUtility.ToJson(data, true);
				File.WriteAllText(langPath, json);
				File.WriteAllText(SourcePath, converter.Text);
			}
			else
			{
				LocalizeData data = new();
				data.Keys = converter.KeyAndValues.Keys.ToArray();

				foreach (var langName in ANovelEditorConfig.Instance.Localize.Language)
				{
					LanguageData lang = new();
					lang.Default = data.List.Count == 0;
					lang.Language = langName;
					if (lang.Default)
					{
						lang.Lines = converter.KeyAndValues.Values.ToArray();
					}
					else
					{
						lang.Lines = converter.KeyAndValues.Values.Select(x => "").ToArray();
					}
					data.List.Add(lang);
				}

				var json = JsonUtility.ToJson(data, true);
				File.WriteAllText(langPath, json);
				File.WriteAllText(SourcePath, converter.Text);
			}
			AssetDatabase.Refresh();
		}

	}

}