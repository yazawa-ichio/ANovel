using ANovel.Core.Define;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ANovel.Engine.Tools
{
	public class ANovelProjectExporter
	{
		public static void ExportDefine(EngineConfig config)
		{
			var project = ProjectDefine.Create(new List<string>(config.Symbols));
			project.Paths = config.Path.GetDefines();
			project.ScenarioPath = config.ProjectExport.ScenarioPath;
			project.ResourcePath = config.ProjectExport.ResourcePath;
			var dirPath = config.ProjectExport.ProjectRoot;
			if (string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
			{
				Debug.LogFormat("出力先が見つかりません。 ProjectRoot:{0}", dirPath);
				return;
			}
			var path = Path.Combine(dirPath, "ANovelProject");
			File.WriteAllText(path, JsonUtility.ToJson(project, true));
			Debug.LogFormat("ExportDefine {0}", path);
		}
	}

	[CustomEditor(typeof(EngineConfig))]
	public class EngineConfigEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("Export Define"))
			{
				ANovelProjectExporter.ExportDefine(target as EngineConfig);
			}
		}


	}

}