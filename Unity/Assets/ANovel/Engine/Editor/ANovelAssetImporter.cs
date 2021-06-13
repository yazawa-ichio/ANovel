using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace ANovel.Engine.Tools
{
	[ScriptedImporter(1, "anovel")]
	public class ANovelAssetImporter : ScriptedImporter
	{
		public override void OnImportAsset(AssetImportContext ctx)
		{
			var texts = File.ReadAllText(ctx.assetPath);
			var asset = new TextAsset(texts);
			ctx.AddObjectToAsset("main", asset);
			ctx.SetMainObject(asset);
		}
	}
}