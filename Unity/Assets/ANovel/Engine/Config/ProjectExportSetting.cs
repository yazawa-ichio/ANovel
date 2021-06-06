using UnityEngine;

namespace ANovel
{
	[System.Serializable]
	public class ProjectExportSetting
	{
		[SerializeField]
		string m_ProjectRoot;
		[SerializeField]
		string m_ScenarioPath;
		[SerializeField]
		string m_ResourcePath;
		[SerializeField]
		string m_DocumentRoot;

		public string ProjectRoot => m_ProjectRoot;

		public string ScenarioPath => m_ScenarioPath;

		public string ResourcePath => m_ResourcePath;

		public string DocumentRoot => m_DocumentRoot;
	}

}