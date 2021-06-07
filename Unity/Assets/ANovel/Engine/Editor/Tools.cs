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

	}
}