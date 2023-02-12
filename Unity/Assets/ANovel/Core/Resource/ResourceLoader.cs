using System.IO;
using System.Threading;
using System.Threading.Tasks;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
using Object = UnityEngine.Object;
#endif

namespace ANovel.Core
{
	public class ResourceLoader : IResourceLoader
	{
		string m_Root;

		public bool StrictUnloadMode { get; set; }

		public int UnloadCounter { get; set; } = 32;

		public ResourceLoader(string root)
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
#if UNITY_5_3_OR_NEWER
		public Task<T> Load<T>(string path, CancellationToken token) where T : Object
		{
			path = GetPath(path);
			TaskCompletionSource<T> future = new TaskCompletionSource<T>();
			var op = Resources.LoadAsync<T>(path);
			op.completed += (_) =>
			{
				var asset = op.asset as T;
				if (asset != null)
				{
					future.TrySetResult(asset);
				}
				else
				{
					future.TrySetException(new FileNotFoundException($"not found Asset: {path}", path));
				}
			};
			return future.Task;
		}
#endif

		public Task<T> LoadRaw<T>(string path, CancellationToken token)
		{
			path = GetPath(path);
			return Task.FromResult((T)(object)Resources.Load(path, typeof(T)));
		}

		int m_UnloadCount;
		public void Unload(string path, object obj)
		{
			if (obj is Object asset)
			{
				if (StrictUnloadMode)
				{
					Resources.UnloadAsset(asset);
				}
				else if (m_UnloadCount++ >= UnloadCounter)
				{
					m_UnloadCount = 0;
					Resources.UnloadUnusedAssets();
				}
			}
		}

	}

}