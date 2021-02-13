namespace ANovel.Core
{
	public interface IPreLoader
	{
#if UNITY_5_3_OR_NEWER
		void Load<T>(string path) where T : UnityEngine.Object;
#endif
		void LoadRaw<T>(string path) where T : class;
	}
}