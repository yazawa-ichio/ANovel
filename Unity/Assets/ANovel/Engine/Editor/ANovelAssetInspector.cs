using ANovel.Core;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace ANovel.Engine.Tools
{
	[CustomEditor(typeof(ANovelAssetImporter))]
	public class ANovelAssetEditor : ScriptedImporterEditor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("Create Localize Data"))
			{
				var langs = ANovelEditorConfig.Instance.Localize.Language.ToArray();
				var path = (target as ANovelAssetImporter).assetPath;
				LocalizeDefineConverter.ApplyDefault(path, langs);
				AssetDatabase.Refresh();
			}
		}
	}
}