using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ANovel.Engine.Tools
{

	public static class Tools
	{
		static readonly string s_EngineId = "1037d35e3eb9eee4e9cedb861a4d313e";

		[MenuItem("GameObject/ANovel/Engine")]
		static void CreateInScene()
		{
			var path = AssetDatabase.GUIDToAssetPath(s_EngineId);
			var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			var obj = PrefabUtility.InstantiatePrefab(prefab);
			PrefabUtility.UnpackPrefabInstance((GameObject)obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		}

		[MenuItem("ANovel/Import/QuickStart")]
		static void ImportQuickStart()
		{
			var name = "ANovelQuickStart";
			var ext = ".unitypackage";
			var paths = AssetDatabase.FindAssets(name).Select(x => AssetDatabase.GUIDToAssetPath(x));
			foreach (var path in paths)
			{
				if (System.IO.Path.GetExtension(path) == ext)
				{
					AssetDatabase.ImportPackage(path, true);
					return;
				}
			}
		}

	}
}