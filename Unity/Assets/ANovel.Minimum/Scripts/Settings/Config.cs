using UnityEngine;

namespace ANovel.Minimum
{
	[CreateAssetMenu(menuName = "ANovel/Minimum/Create Config")]
	public class Config : ScriptableObject
	{
		[SerializeField]
		string m_ImageRoot = "Image";
		[SerializeField]
		string m_BgmRoot = "Bgm";
		[SerializeField]
		string m_SeRoot = "Se";
		[SerializeField]
		float m_TextSpeed = 0.02f;
		[SerializeField]
		float m_AutoWait = 1.5f;

		public float TextSpeed
		{
			get => m_TextSpeed;
			set => m_TextSpeed = value;
		}

		public float AutoWait
		{
			get => m_AutoWait;
			set => m_AutoWait = value;
		}

		string GetPath(string root, string path)
		{
			if (string.IsNullOrEmpty(root))
			{
				return path;
			}
			else
			{
				return root + "/" + path;
			}
		}

		public string GetImagePath(string path)
		{
			return GetPath(m_ImageRoot, path);
		}

		public string GetSePath(string path)
		{
			return GetPath(m_SeRoot, path);
		}

		public string GetBgmPath(string path)
		{
			return GetPath(m_BgmRoot, path);
		}

	}
}