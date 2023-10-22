using System;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace ANovel.Engine.Tools
{
	[CustomEditor(typeof(ANovelLangAssetImporter))]
	public class ANovelLangAssetEditor : ScriptedImporterEditor
	{
		[Serializable]
		class SheetSetting
		{
			public string SheetId;
			public string SheetName;
		}

		string m_UserData;
		SheetSetting m_SheetSetting;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			var importer = (target as ANovelLangAssetImporter);
			if (importer == null)
			{
				return;
			}
			DrawUserData(importer);
			if (GUILayout.Button("Push Spreadsheet"))
			{
				LocalizeSpreadsheetSync.Push(importer.assetPath, m_SheetSetting.SheetId, m_SheetSetting.SheetName);
			}
			if (GUILayout.Button("Pull Spreadsheet"))
			{
				LocalizeSpreadsheetSync.Pull(importer.assetPath, m_SheetSetting.SheetId, m_SheetSetting.SheetName);
			}
		}

		void DrawUserData(ANovelLangAssetImporter importer)
		{
			if (m_UserData != importer.userData)
			{
				m_UserData = importer.userData;
				if (string.IsNullOrEmpty(m_UserData))
				{
					m_SheetSetting = new SheetSetting();
				}
				else
				{
					m_SheetSetting = JsonUtility.FromJson<SheetSetting>(m_UserData);
				}
			}
			using (var scope = new EditorGUI.ChangeCheckScope())
			{
				m_SheetSetting.SheetId = EditorGUILayout.DelayedTextField("SheetId", m_SheetSetting.SheetId);
				m_SheetSetting.SheetName = EditorGUILayout.DelayedTextField("SheetName", m_SheetSetting.SheetName);
				if (scope.changed)
				{
					m_UserData = importer.userData = JsonUtility.ToJson(m_SheetSetting);
					importer.SaveAndReimport();
				}
			}
		}
	}
}