using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ANovel.Engine.Tools
{
	[System.Serializable]
	public class LocalizeSettings
	{
		public List<string> Language = new()
		{
			"ja",
			"en"
		};
	}

	[System.Serializable]
	public class ANovelEditorConfig : ScriptableObject
	{

		static readonly string s_Path = "ProjectSettings/ANovelEditorConfig.json";

		static ANovelEditorConfig s_Instance;

		public static ANovelEditorConfig Instance
		{
			get
			{
				if (s_Instance == null)
				{
					s_Instance = ScriptableObject.CreateInstance<ANovelEditorConfig>();
					if (File.Exists(s_Path))
					{
						var json = File.ReadAllText(s_Path);
						JsonUtility.FromJsonOverwrite(json, s_Instance);
					}
				}
				return s_Instance;
			}
		}

		public LocalizeSettings Localize = new();

		private ANovelEditorConfig() { }

		public void Save()
		{
			var json = JsonUtility.ToJson(this, true);
			File.WriteAllText(s_Path, json);
		}

	}
}