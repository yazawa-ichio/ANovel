using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	internal class PrefixedEnvData : IEnvData
	{

		string m_Prefix;
		IEnvData m_Data;

		public PrefixedEnvData(string prefix, IEnvData data)
		{
			m_Prefix = prefix;
			m_Data = data;
		}

		public IEnvData GetParent() => m_Data;

		IEnvDataHolder IEnvDataHolder.GetParent() => m_Data;

		public void Delete<TValue>(string key) where TValue : struct, IEnvValue
		{
			m_Data.Delete<TValue>(m_Prefix + key);
		}

		public void DeleteAll<TValue>(Func<string, TValue, bool> func) where TValue : struct, IEnvValue
		{
			Func<string, TValue, bool> wrapFunc = null;
			if (func != null)
			{
				wrapFunc = (key, value) =>
				{
					if (!key.StartsWith(m_Prefix, StringComparison.Ordinal))
					{
						return false;
					}
					key = key.Substring(m_Prefix.Length);
					return func(key, value);
				};
			}
			m_Data.DeleteAll(wrapFunc);
		}

		public void DeleteAllByInterface<TInterface>(Func<string, TInterface, bool> func) where TInterface : class
		{
			Func<string, TInterface, bool> wrapFunc = null;
			if (func != null)
			{
				wrapFunc = (key, value) =>
				{
					if (!key.StartsWith(m_Prefix, StringComparison.Ordinal))
					{
						return false;
					}
					key = key.Substring(m_Prefix.Length);
					return func(key, value);
				};
			}
			m_Data.DeleteAllByInterface(wrapFunc);
		}

		public TValue Get<TValue>(string key) where TValue : struct, IEnvValue
		{
			return m_Data.Get<TValue>(m_Prefix + key);
		}

		public IEnumerable<KeyValuePair<string, TValue>> GetAll<TValue>() where TValue : struct, IEnvValue
		{
			foreach (var kvp in m_Data.GetAll<TValue>())
			{
				if (kvp.Key.StartsWith(m_Prefix, StringComparison.Ordinal))
				{
					yield return new KeyValuePair<string, TValue>(kvp.Key.Substring(m_Prefix.Length), kvp.Value);
				}
			}
		}

		public bool Has<TValue>(string key) where TValue : struct, IEnvValue
		{
			return m_Data.Has<TValue>(m_Prefix + key);
		}

		public void Set<TValue>(string key, TValue value) where TValue : struct, IEnvValue
		{
			m_Data.Set(m_Prefix + key, value);
		}

		public bool TryGet<TValue>(string key, out TValue value) where TValue : struct, IEnvValue
		{
			return m_Data.TryGet(m_Prefix + key, out value);
		}

		public IEnumerable<KeyValuePair<string, TInterface>> GetAllByInterface<TInterface>() where TInterface : class
		{
			foreach (var kvp in m_Data.GetAllByInterface<TInterface>())
			{
				if (kvp.Key.StartsWith(m_Prefix, StringComparison.Ordinal))
				{
					yield return new KeyValuePair<string, TInterface>(kvp.Key.Substring(m_Prefix.Length), kvp.Value);
				}
			}
		}
	}

	public class PrefixedEnvDataHolder : IEnvDataHolder
	{

		string m_Prefix;
		IEnvDataHolder m_Data;

		public PrefixedEnvDataHolder(string prefix, IEnvDataHolder data)
		{
			m_Prefix = prefix;
			m_Data = data;
		}

		public IEnvDataHolder GetParent() => m_Data;

		public TValue Get<TValue>(string key) where TValue : struct, IEnvValue
		{
			return m_Data.Get<TValue>(m_Prefix + key);
		}

		public IEnumerable<KeyValuePair<string, TValue>> GetAll<TValue>() where TValue : struct, IEnvValue
		{
			foreach (var kvp in m_Data.GetAll<TValue>())
			{
				if (kvp.Key.StartsWith(m_Prefix, StringComparison.Ordinal))
				{
					yield return new KeyValuePair<string, TValue>(kvp.Key.Substring(m_Prefix.Length), kvp.Value);
				}
			}
		}

		public bool Has<TValue>(string key) where TValue : struct, IEnvValue
		{
			return m_Data.Has<TValue>(m_Prefix + key);
		}

		public bool TryGet<TValue>(string key, out TValue value) where TValue : struct, IEnvValue
		{
			return m_Data.TryGet(m_Prefix + key, out value);
		}

		public IEnumerable<KeyValuePair<string, TInterface>> GetAllByInterface<TInterface>() where TInterface : class
		{
			foreach (var kvp in m_Data.GetAllByInterface<TInterface>())
			{
				if (kvp.Key.StartsWith(m_Prefix, StringComparison.Ordinal))
				{
					yield return new KeyValuePair<string, TInterface>(kvp.Key.Substring(m_Prefix.Length), kvp.Value);
				}
			}
		}
	}



}