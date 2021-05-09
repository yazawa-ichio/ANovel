using UnityEngine;

namespace ANovel
{
	public class EngineConfig : ScriptableObject
	{
		[SerializeField]
		PathConfig m_Path = new PathConfig();

		public PathConfig Path => m_Path;

	}

	[System.Serializable]
	public class PathConfig
	{
		[SerializeField]
		string m_CharaRoot = "Chara";
		[SerializeField]
		string m_CharaFaceRoot = "Chara";
		[SerializeField]
		string m_BgRoot = "Image/Bg";
		[SerializeField]
		string m_ImageRoot = "Image";
		[SerializeField]
		string m_RuleRoot = "Image/Rule";
		[SerializeField]
		string m_BgmRoot = "Sound/BGM";
		[SerializeField]
		string m_SeRoot = "Sound/SE";

		public string BgRoot => GetBg("");

		public string ImageRoot => GetImage("");

		public string RuleRoot => GetRule("");

		public string BgmRoot => GetBgm("");

		public string CharaRoot => GetChara("");

		public string CharaFaceWindowRoot => GetCharaFace("");

		public string SeRoot => GetSe("");

		public string GetPath(string root, string path)
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

		public string GetChara(string path)
		{
			return GetPath(m_CharaRoot, path);
		}

		public string GetCharaFace(string path)
		{
			return GetPath(m_CharaFaceRoot, path);
		}

		public string GetBg(string path)
		{
			return GetPath(m_BgRoot, path);
		}

		public string GetImage(string path)
		{
			return GetPath(m_ImageRoot, path);
		}

		public string GetRule(string path)
		{
			return GetPath(m_RuleRoot, path);
		}

		public string GetSe(string path)
		{
			return GetPath(m_SeRoot, path);
		}

		public string GetBgm(string path)
		{
			return GetPath(m_BgmRoot, path);
		}

	}
}