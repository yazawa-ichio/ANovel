using System;
using System.Collections.Generic;
using System.Threading;

namespace ANovel.Core
{

	public class ResourceCache : IResourceCache, IDisposable
	{
		Queue<string> m_Release = new Queue<string>();
		Dictionary<string, CacheEntry> m_Dic = new Dictionary<string, CacheEntry>();
		IResourceLoader m_Loader;

		public ResourceCache(IResourceLoader loader)
		{
			m_Loader = loader;
		}

		public ICacheHandle<T> Load<T>(string path) where T : UnityEngine.Object
		{
			if (m_Dic.TryGetValue(path, out var value))
			{
				if (value is CacheEntry<T> entry)
				{
					return new CacheHandle<T>(entry);
				}
				throw new CacheException(path, $"different cache type. Load<{typeof(T)}>  Cache:{value.CacheType}");
			}
			else
			{
				var source = new CancellationTokenSource();
				var task = m_Loader.Load<T>(path, source.Token);
				var entry = new CacheEntry<T>(path, m_Loader, source, task);
				m_Dic[path] = entry;
				return new CacheHandle<T>(entry);
			}
		}

		public ICacheHandle<T> LoadRaw<T>(string path) where T : class
		{
			if (m_Dic.TryGetValue(path, out var value))
			{
				if (value is CacheEntry<T> entry)
				{
					return new CacheHandle<T>(entry);
				}
				throw new CacheException(path, $"different cache type. LoadRaw<{typeof(T)}>  Cache:{value.CacheType}");
			}
			else
			{
				var source = new CancellationTokenSource();
				var task = m_Loader.LoadRaw<T>(path, source.Token);
				var entry = new CacheEntry<T>(path, m_Loader, source, task);
				m_Dic[path] = entry;
				return new CacheHandle<T>(entry);
			}
		}

		public T Get<T>(string path) where T : class
		{
			if (m_Dic.TryGetValue(path, out var value) && value.IsLoaded)
			{
				if (value is CacheEntry<T> entry)
				{
					return entry.Value;
				}
			}
			throw new CacheException(path, "dont cache. use Load method");
		}

		public void ReleaseUnused()
		{
			foreach (var kvp in m_Dic)
			{
				if (kvp.Value.RefCount == 0)
				{
					m_Release.Enqueue(kvp.Key);
					kvp.Value.Dispose();
				}
			}
			while (m_Release.Count > 0)
			{
				m_Dic.Remove(m_Release.Dequeue());
			}
		}

		public void Dispose()
		{
			foreach (var entry in m_Dic.Values)
			{
				entry.Dispose();
			}
			m_Dic.Clear();
			m_Loader?.Dispose();
			m_Loader = null;
		}

	}


}