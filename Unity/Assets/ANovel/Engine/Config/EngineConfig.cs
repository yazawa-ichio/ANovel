using System.Linq;
using UnityEngine;

namespace ANovel
{
	[System.Serializable]
	[CreateAssetMenu(fileName = nameof(EngineConfig), menuName = "ANovel/" + nameof(EngineConfig))]
	public class EngineConfig : ScriptableObject
	{
		public static readonly string[] DefaultSymbols = new string[]
		{
			"ANovelEngine",
		};

		[SerializeField]
		bool m_DefaultSymbol = true;
		[SerializeField]
		string[] m_CustomSymbols = System.Array.Empty<string>();
		[SerializeField]
		ProjectExportSetting m_ProjectExport = new ProjectExportSetting();
		[SerializeField]
		PathConfig m_Path = new PathConfig();

		public string[] Symbols => m_DefaultSymbol ? DefaultSymbols.Concat(m_CustomSymbols).ToArray() : m_CustomSymbols;

		public ProjectExportSetting ProjectExport => m_ProjectExport;

		public PathConfig Path => m_Path;
	}

}