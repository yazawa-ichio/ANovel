using System.Collections.Generic;

namespace ANovel
{
	public interface IMetaData
	{
		IEnumerable<KeyValuePair<string, T>> GetAll<T>();
		T Get<T>(string key);
		bool TryGet<T>(string key, out T value);
		T GetSingle<T>();
		bool TryGetSingle<T>(out T value);
	}
}