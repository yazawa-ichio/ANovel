using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public class LocalScenarioLoader : IScenarioLoader
	{
		string m_Root;

		public LocalScenarioLoader(string root)
		{
			m_Root = root;
		}

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
#if !UNITY_WEBGL
			var text = System.IO.File.ReadAllText(GetPath(path));
			return Task.FromResult(text);
#else
			TaskCompletionSource<string> future = new TaskCompletionSource<string>();
			var req = UnityEngine.Networking.UnityWebRequest.Get(GetPath(path));
			req.SendWebRequest().completed += (_) =>
			{
				if (req.isNetworkError || req.isHttpError)
				{
					future.TrySetException(new System.Exception(req.error));

				}
				else
				{
					future.TrySetResult(req.downloadHandler.text);
				}
			};
			return future.Task;
#endif
		}

		public void Dispose() { }

	}

}