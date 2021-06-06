using System.Collections.Generic;

namespace ANovel.Core.Define
{

	[System.Serializable]
	public class ProjectDefine
	{
		public string ScenarioPath;

		public string ResourcePath;

		public TagDefine[] Tags;

		public CompletionItemDefine CompletionItem;

		public PathDefine[] Paths;

		public static ProjectDefine Create(List<string> symbols)
		{
			var project = new ProjectDefine();
			project.Tags = TagDefine.GetDefines(symbols);
			project.CompletionItem = CompletionItemDefine.Get(symbols);
			return project;
		}

	}

}