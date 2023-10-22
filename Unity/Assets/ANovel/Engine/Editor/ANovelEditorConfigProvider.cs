using System.Collections.Generic;
using UnityEditor;

namespace ANovel.Engine.Tools
{
	public class ANovelEditorConfigProvider : SettingsProvider
	{

		[SettingsProvider]
		public static SettingsProvider CreateProvider()
		{
			var provider = new ANovelEditorConfigProvider("Project/ANovelConfig", SettingsScope.Project);
			return provider;
		}

		SerializedObject m_SerializedObject;


		public ANovelEditorConfigProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
		{
		}

		public override void OnGUI(string searchContext)
		{
			var instance = ANovelEditorConfig.Instance;
			if (m_SerializedObject == null)
			{
				m_SerializedObject = new SerializedObject(instance);
			}
			m_SerializedObject.Update();
			using (var scope = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.LabelField("ANovelConfig");
				EditorGUILayout.PropertyField(m_SerializedObject.FindProperty("Localize"), true);
				if (scope.changed)
				{
					m_SerializedObject.ApplyModifiedProperties();
					instance.Save();
				}
			}
		}
	}
}