using System.Linq;
using UnityEditor;

namespace ANovel.TestTools
{
	public class ReleaseTool
	{
		[MenuItem("ANovel/Release/ExportPackages")]
		static void ExportPackages()
		{
			var fileName = "Assets/ANovel/Unity/Packages/ANovelQuickStart.unitypackage";
			var paths = AssetDatabase.GetAllAssetPaths().Where(x => x.StartsWith("Assets/ANovel.QuickStart"));
			AssetDatabase.ExportPackage(paths.ToArray(), fileName, ExportPackageOptions.Default);
		}
	}
}