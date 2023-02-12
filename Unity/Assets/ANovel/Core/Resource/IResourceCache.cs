namespace ANovel
{
	public interface IResourceCache
	{
		ICacheHandle<T> Load<T>(string path) where T : UnityEngine.Object;
		ICacheHandle<T> LoadRaw<T>(string path) where T : class;
		T Get<T>(string path) where T : class;
	}
}