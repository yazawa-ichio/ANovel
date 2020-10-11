using System.Threading;
using System.Threading.Tasks;

namespace ANovel
{
	public interface IResourceLoader
	{
#if UNITY_5_3_OR_NEWER
		Task<T> Load<T>(string path, CancellationToken token) where T : UnityEngine.Object;
#endif
		Task<T> LoadRaw<T>(string path, CancellationToken token);
	}
}