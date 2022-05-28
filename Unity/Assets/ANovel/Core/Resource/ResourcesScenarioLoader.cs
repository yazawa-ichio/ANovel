using System.IO;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

namespace ANovel.Core
{
#if UNITY_5_3_OR_NEWER
	public class ResourcesScenarioLoader : IScenarioLoader
	{
		string m_Root;

		public ResourcesScenarioLoader(string root)
		{
			m_Root = root;
		}

		public void Dispose() { }

		string GetPath(string path)
		{
			if (string.IsNullOrEmpty(m_Root))
			{
				return path;
			}
			else
			{
				return m_Root + "/" + path;
			}
		}

		public Task<string> Load(string path, CancellationToken token)
		{
			path = GetPath(path);
			var text = Resources.Load<TextAsset>(path);
			if (text == null)
			{
				throw new FileNotFoundException($"not found Asset {path}", path);
			}
			return Task.FromResult(text.text);
		}

	}
#endif

}