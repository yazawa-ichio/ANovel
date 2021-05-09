using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public interface IMetaData
	{
		IEnumerable<KeyValuePair<string, T>> GetAll<T>();
		T Get<T>(string key);
		bool TryGet<T>(string key, out T value);
		T GetSingle<T>();
		bool TryGetSingle<T>(out T value);
	}

	public class MetaData : IMetaData
	{

		Dictionary<Type, object> m_Dic = new Dictionary<Type, object>();
		List<MetaData> m_Depend = new List<MetaData>();

		Dictionary<string, T> GetDic<T>()
		{
			if (!m_Dic.TryGetValue(typeof(T), out var dic))
			{
				dic = m_Dic[typeof(T)] = new Dictionary<string, T>();
			}
			return (Dictionary<string, T>)dic;
		}

		public void Depend(MetaData meta)
		{
			m_Depend.Add(meta);
		}

		public void Set<T>(string key, T value)
		{
			GetDic<T>()[key] = value;
		}

		public void SetSingle<T>(T value)
		{
			GetDic<T>()[nameof(T)] = value;
		}

		public void Add<T>(string key, T value)
		{
			GetDic<T>().Add(key, value);
		}

		public void AddSingle<T>(T value)
		{
			GetDic<T>().Add(nameof(T), value);
		}

		public IEnumerable<KeyValuePair<string, T>> GetAll<T>()
		{
			return GetAllImpl<T>(null, null);
		}

		IEnumerable<KeyValuePair<string, T>> GetAllImpl<T>(List<string> keys, List<MetaData> metas)
		{
			bool first = keys == null;
			keys = keys ?? ListPool<string>.Pop();
			metas = metas ?? ListPool<MetaData>.Pop();
			if (metas.Contains(this))
			{
				yield break;
			}
			metas.Add(this);
			foreach (var kvp in GetDic<T>())
			{
				if (!keys.Contains(kvp.Key))
				{
					keys.Add(kvp.Key);
					yield return kvp;
				}
			}
			foreach (var dep in m_Depend)
			{
				foreach (var kvp in dep.GetAllImpl<T>(keys, metas))
				{
					yield return kvp;
				}
			}
			if (first)
			{
				ListPool<string>.Push(keys);
				ListPool<MetaData>.Push(metas);
			}
		}

		public T GetOrCreateSingle<T>() where T : new()
		{
			using (ListPool<MetaData>.Use(out var list))
			{
				if (TryGetImpl(list, nameof(T), out T value))
				{
					return value;
				}
			}
			{
				var value = new T();
				AddSingle(value);
				return value;
			}
		}

		public T GetSingle<T>()
		{
			using (ListPool<MetaData>.Use(out var list))
			{
				if (TryGetImpl(list, nameof(T), out T value))
				{
					return value;
				}
			}
			throw new KeyNotFoundException($"not found MetaData<{typeof(T)}> key:{nameof(T)}");
		}

		public bool TryGetSingle<T>(out T value)
		{
			using (ListPool<MetaData>.Use(out var list))
			{
				if (TryGetImpl(list, nameof(T), out value))
				{
					return true;
				}
			}
			return false;
		}

		public T Get<T>(string key)
		{
			if (TryGet<T>(key, out var val))
			{
				return val;
			}
			throw new KeyNotFoundException($"not found MetaData<{typeof(T)}> key:{key}");
		}

		public bool TryGet<T>(string key, out T value)
		{
			using (ListPool<MetaData>.Use(out var list))
			{
				return TryGetImpl(list, key, out value);
			}
		}

		bool TryGetImpl<T>(List<MetaData> check, string key, out T value)
		{
			if (GetDic<T>().TryGetValue(key, out value))
			{
				return true;
			}
			if (check.Contains(this))
			{
				return false;
			}
			check.Add(this);
			foreach (var dep in m_Depend)
			{
				if (dep.TryGetImpl(check, key, out value))
				{
					return true;
				}
			}
			return false;
		}

	}
}