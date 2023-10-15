using UnityEditor;

namespace ANovel.GoogleApis
{

	public class ProjectSettingsProvider : SettingsProvider
	{
		public static string ProjectSettingsPath => "ProjectSettings/ANovel.GoogleApis.";

		[SettingsProvider]
		public static SettingsProvider CreateProvider()
		{
			var provider = new ProjectSettingsProvider("Project/ANovel GoogleApis", SettingsScope.Project);
			return provider;
		}

		public ProjectSettingsProvider(string path, SettingsScope scope) : base(path, scope) { }

		public override void OnGUI(string searchContext)
		{
			var instance = GoogleApisSettings.Instance;
			using (var scope = new EditorGUI.ChangeCheckScope())
			{
				EditorGUILayout.LabelField("Credentials");
				using (new EditorGUI.IndentLevelScope())
				{
					instance.Credentials.ClientId = EditorGUILayout.TextField("ClientId", instance.Credentials.ClientId);
					instance.Credentials.ClientSecret = EditorGUILayout.TextField("ClientSecret", instance.Credentials.ClientSecret);
				}
				if (scope.changed)
				{
					instance.Save();
				}
			}
		}

	}
}