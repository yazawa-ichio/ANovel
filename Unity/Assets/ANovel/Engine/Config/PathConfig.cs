using ANovel.Core.Define;
using System.Collections.Generic;
using UnityEngine;

namespace ANovel
{
	[System.Serializable]
	public class PathConfig
	{
		[SerializeField]
		string m_CharaRoot = "Chara/";
		[SerializeField]
		string m_CharaFaceWindowRoot = "Chara/";
		[SerializeField]
		string m_BgRoot = "Image/Bg/";
		[SerializeField]
		string m_ImageRoot = "Image/";
		[SerializeField]
		string m_RuleRoot = "Image/Rule/";
		[SerializeField]
		string m_BgmRoot = "Sound/BGM/";
		[SerializeField]
		string m_SeRoot = "Sound/SE/";
		[SerializeField]
		string m_VoiceRoot = "Sound/Voice/";

		string GetPathImpl(string root, string path)
		{
			if (string.IsNullOrEmpty(root))
			{
				return path;
			}
			else if (root.EndsWith("/", System.StringComparison.Ordinal))
			{
				return root + path;
			}
			else
			{
				return root + "/" + path;
			}
		}

		public string GetPath(PathCategory category, string path)
		{
			return GetPathImpl(GetRoot(category), path);
		}

		public string GetPath(string prefix, string path)
		{
			return GetPathImpl(prefix, path);
		}

		public string GetRoot(PathCategory category)
		{
			switch (category)
			{
				case PathCategory.Chara:
					return m_CharaRoot;
				case PathCategory.CharaFaceWindow:
					return m_CharaFaceWindowRoot;
				case PathCategory.Bg:
					return m_BgRoot;
				case PathCategory.Image:
					return m_ImageRoot;
				case PathCategory.Rule:
					return m_RuleRoot;
				case PathCategory.Bgm:
					return m_BgmRoot;
				case PathCategory.Se:
					return m_SeRoot;
				case PathCategory.Voice:
					return m_VoiceRoot;
			}
			throw new System.ArgumentException($"not found category {category}");
		}

		public PathDefine[] GetDefines()
		{
			var paths = new List<PathDefine>();
			foreach (PathCategory category in System.Enum.GetValues(typeof(PathCategory)))
			{
				paths.Add(new PathDefine
				{
					Category = category.ToString(),
					RootPath = GetRoot(category)
				});
			}
			return paths.ToArray();
		}

	}
}