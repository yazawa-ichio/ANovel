using UnityEditor;
using UnityEngine;

namespace ANovel.Engine.Tools
{

	[CustomEditor(typeof(ANovelEngine))]
	public class ANovelEngineEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var config = (target as ANovelEngine)?.Config;
			if (config != null)
			{
				if (GUILayout.Button("Export Define"))
				{
					ANovelProjectExporter.ExportDefine(config);
				}
			}
			else
			{
				if (GUILayout.Button("Create Config"))
				{
					var path = EditorUtility.SaveFilePanelInProject("Create Config", nameof(EngineConfig), "asset", "");
					if (string.IsNullOrEmpty(path))
					{
						return;
					}
					config = ScriptableObject.CreateInstance<EngineConfig>();
					AssetDatabase.CreateAsset(config, path);
				}
			}
		}
	}

}