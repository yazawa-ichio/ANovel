using System;
using System.Collections.Generic;

namespace ANovel.Core
{

	public class EnvData : IEnvData
	{
		static EnvData s_Empty = new EnvData();
		internal static EnvData Empty { get { s_Empty.Clear(); return s_Empty; } }

		Dictionary<Type, IEnvDataEntry> m_Dic = new Dictionary<System.Type, IEnvDataEntry>();
		HashSet<Type> m_Dirty = new HashSet<Type>();

		EnvDataEntry<TValue> GetEntry<TValue>() where TValue : struct
		{
			m_Dirty.Add(typeof(TValue));
			if (!m_Dic.TryGetValue(typeof(TValue), out var entry))
			{
				m_Dic[typeof(TValue)] = entry = new EnvDataEntry<TValue>();
			}
			return entry as EnvDataEntry<TValue>;
		}

		IEnvData IEnvData.GetParent() => null;

		IEnvDataHolder IEnvDataHolder.GetParent() => null;

		public bool Has<TValue>(string key) where TValue : struct
		{
			return GetEntry<TValue>().Has(key);
		}

		public void Set<TValue>(string key, TValue value) where TValue : struct
		{
			GetEntry<TValue>().Set(key, value);
		}

		public void Delete<TValue>(string key) where TValue : struct
		{
			GetEntry<TValue>().Delete(key);
		}

		public void DeleteAll<TValue>(Func<string, TValue, bool> func) where TValue : struct
		{
			GetEntry<TValue>().DeleteAll(func);
		}

		public void DeleteAllByInterface<TInterface>(Func<string, TInterface, bool> func) where TInterface : class
		{
			foreach (var kvp in m_Dic)
			{
				if (typeof(TInterface).IsAssignableFrom(kvp.Key))
				{
					m_Dirty.Add(kvp.Key);
					if (func == null)
					{
						kvp.Value.DeleteAll(null);
					}
					else
					{
						kvp.Value.DeleteAll((key, val) =>
						{
							return func(key, val as TInterface);
						});
					}
				}
			}
		}

		public TValue Get<TValue>(string key) where TValue : struct
		{
			return GetEntry<TValue>().Get(key);
		}

		public bool TryGet<TValue>(string key, out TValue value) where TValue : struct
		{
			return GetEntry<TValue>().TryGet(key, out value);
		}

		public IEnumerable<KeyValuePair<string, TValue>> GetAll<TValue>() where TValue : struct
		{
			return GetEntry<TValue>().GetAll();
		}

		public EnvDataDiff Diff()
		{
			var diff = new EnvDataDiff();
			foreach (var kvp in m_Dic)
			{
				if (m_Dirty.Remove(kvp.Key))
				{
					kvp.Value.Diff(diff);
				}
			}
			m_Dirty.Clear();
			return diff;
		}

		public void Redo(EnvDataDiff diff)
		{
			EnsureEntry(diff.GetEntryTypes());
			foreach (var entry in m_Dic.Values)
			{
				entry.Redo(diff);
			}
		}

		public void Undo(EnvDataDiff diff)
		{
			EnsureEntry(diff.GetEntryTypes());
			foreach (var entry in m_Dic.Values)
			{
				entry.Undo(diff);
			}
		}

		public EnvDataSnapshot Save()
		{
			var data = new EnvDataSnapshot();
			foreach (var entry in m_Dic.Values)
			{
				entry.Save(data);
			}
			return data;
		}

		public void Load(EnvDataSnapshot data)
		{
			EnsureEntry(data.GetEntryTypes());
			foreach (var entry in m_Dic.Values)
			{
				entry.Load(data);
			}
		}

		public EnvDataSnapshot SaveByInterface<TInterface>()
		{
			var data = new EnvDataSnapshot();
			foreach (var kvp in m_Dic)
			{
				if (typeof(TInterface).IsAssignableFrom(kvp.Key))
				{
					kvp.Value.Save(data);
				}
			}
			return data;
		}

		public void Clear()
		{
			foreach (var kvp in m_Dic)
			{
				m_Dirty.Add(kvp.Key);
				kvp.Value.Clear();
			}
		}

		void EnsureEntry(IEnumerable<Type> types)
		{
			foreach (var type in types)
			{
				EnsureEntry(type);
			}
		}

		void EnsureEntry(Type type)
		{
			if (!m_Dic.ContainsKey(type))
			{
				var entryType = typeof(EnvDataEntry<>).MakeGenericType(type);
				m_Dic[type] = (IEnvDataEntry)Activator.CreateInstance(entryType);
			}
		}


	}



}