using System;
using System.IO;
using UnityEngine;

namespace ANovel.GoogleApis
{
	[Serializable]
	public class GoogleApisSettings
	{
		static readonly string s_Path = "ProjectSettings/ANovel.GoogleApis.GoogleApisSettings.json";

		static GoogleApisSettings s_Instance;

		public static GoogleApisSettings Instance
		{
			get
			{
				if (s_Instance == null)
				{
					s_Instance = new GoogleApisSettings();
					if (File.Exists(s_Path))
					{
						var json = File.ReadAllText("ProjectSettings/ANovel.GoogleApis.GoogleApisSettings.json");
						JsonUtility.FromJsonOverwrite(json, s_Instance);
					}
				}
				return s_Instance;
			}
		}

		private GoogleApisSettings() { }

		public int RedirectUrlPorts = 53414;

		public Credentials Credentials = new();

		public void Save()
		{
			var json = JsonUtility.ToJson(this, true);
			File.WriteAllText(s_Path, json);
		}

	}
}