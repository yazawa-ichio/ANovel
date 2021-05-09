using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{
	internal interface IEnvDataEntry
	{
		void Diff(EnvDataDiff diff);
		void Redo(EnvDataDiff diff);
		void Undo(EnvDataDiff diff);
		void Save(EnvDataSnapshot data);
		void Load(EnvDataSnapshot data);
		void DeleteAll(Func<string, object, bool> func);
		void Clear();
	}

	[UnityEngine.Scripting.Preserve]
	internal class EnvDataEntry<TValue> : IEnvDataEntry where TValue : struct
	{
		Dictionary<string, TValue> m_Dic = new Dictionary<string, TValue>();
		Dictionary<string, TValue> m_Prev = new Dictionary<string, TValue>();
		HashSet<string> m_Ditry = new HashSet<string>();

		public bool Has(string key)
		{
			return m_Dic.ContainsKey(key);
		}

		public void Set(string key, TValue value)
		{
			m_Dic[key] = value;
			m_Ditry.Add(key);
		}

		public void Delete(string key)
		{
			m_Dic.Remove(key);
		}

		public void DeleteAll(Func<string, TValue, bool> func)
		{
			if (func == null)
			{
				m_Dic.Clear();
				return;
			}
			using (ListPool<string>.Use(out var temp))
			{
				foreach (var kvp in m_Dic)
				{
					if (func(kvp.Key, kvp.Value))
					{
						temp.Add(kvp.Key);
					}
				}
				foreach (var key in temp)
				{
					m_Dic.Remove(key);
				}
			}
		}

		public void DeleteAll(Func<string, object, bool> func)
		{
			if (func == null)
			{
				m_Dic.Clear();
				return;
			}
			using (ListPool<string>.Use(out var temp))
			{
				foreach (var kvp in m_Dic)
				{
					if (func(kvp.Key, kvp.Value))
					{
						temp.Add(kvp.Key);
					}
				}
				foreach (var key in temp)
				{
					m_Dic.Remove(key);
				}
			}
		}

		public TValue Get(string key)
		{
			return m_Dic[key];
		}

		public bool TryGet(string key, out TValue value)
		{
			return m_Dic.TryGetValue(key, out value);
		}

		public IEnumerable<KeyValuePair<string, TValue>> GetAll()
		{
			return m_Dic;
		}

		public void Diff(EnvDataDiff diff)
		{
			var data = Diff();
			if (data.HasData)
			{
				diff.Set(data);
			}
		}

		public DiffData<TValue> Diff()
		{
			try
			{
				using (ListPool<string>.Use(out var temp))
				using (ListPool<NewDiffData<TValue>>.Use(out var newList))
				using (ListPool<UpdateDiffData<TValue>>.Use(out var updataList))
				using (ListPool<DeleteDiffData<TValue>>.Use(out var deleteList))
				{
					var cur = m_Dic;
					var prev = m_Prev;
					temp.AddRange(cur.Keys);
					foreach (var kvp in prev)
					{
						temp.Remove(kvp.Key);
						if (cur.TryGetValue(kvp.Key, out var value))
						{
							if (m_Ditry.Remove(kvp.Key) && !value.Equals(kvp.Value))
							{
								updataList.Add(new UpdateDiffData<TValue>(kvp.Key, kvp.Value, value));
							}
						}
						else
						{
							deleteList.Add(new DeleteDiffData<TValue>(kvp.Key, kvp.Value));
						}
					}
					foreach (var key in temp)
					{
						newList.Add(new NewDiffData<TValue>(key, cur[key]));
					}
					if (newList.Count == 0 && updataList.Count == 0 && deleteList.Count == 0)
					{
						return DiffData<TValue>.Empty;
					}
					return new DiffData<TValue>
					{
						New = newList.ToArray(),
						Update = updataList.ToArray(),
						Delete = deleteList.ToArray(),
					};
				}
			}
			finally
			{
				CopyToPrev();
			}
		}

		void CopyToPrev()
		{
			m_Prev.Clear();
			foreach (var kvp in m_Dic)
			{
				m_Prev[kvp.Key] = kvp.Value;
			}
			m_Ditry.Clear();
		}

		public void Redo(EnvDataDiff diff)
		{
			Redo(diff.Get<TValue>());
		}

		public void Redo(DiffData<TValue> diff)
		{
			try
			{
				foreach (var data in diff.New)
				{
					m_Dic[data.Key] = data.Value;
				}
				foreach (var data in diff.Update)
				{
					m_Dic[data.Key] = data.Current;
				}
				foreach (var data in diff.Delete)
				{
					m_Dic.Remove(data.Key);
				}
			}
			finally
			{
				CopyToPrev();
			}
		}

		public void Undo(EnvDataDiff diff)
		{
			Undo(diff.Get<TValue>());
		}

		public void Undo(DiffData<TValue> diff)
		{
			try
			{
				foreach (var data in diff.New)
				{
					m_Dic.Remove(data.Key);
				}
				foreach (var data in diff.Update)
				{
					m_Dic[data.Key] = data.Prev;
				}
				foreach (var data in diff.Delete)
				{
					m_Dic[data.Key] = data.Value;
				}
			}
			finally
			{
				CopyToPrev();
			}
		}

		public void Save(EnvDataSnapshot data)
		{
			if (m_Dic.Count == 0)
			{
				return;
			}
			var ret = new Dictionary<string, TValue>(m_Dic.Count);
			foreach (var key in m_Dic.Keys.OrderBy(x => x))
			{
				ret[key] = m_Dic[key];
			}
			data.Set(ret);
		}

		public void Load(EnvDataSnapshot data)
		{
			m_Dic.Clear();
			m_Prev.Clear();
			m_Ditry.Clear();
			var snashort = data.Get<TValue>();
			if (snashort == null)
			{
				return;
			}
			foreach (var kvp in snashort)
			{
				m_Dic[kvp.Key] = kvp.Value;
				m_Prev[kvp.Key] = kvp.Value;
			}
		}

		public void Clear()
		{
			m_Dic.Clear();
			m_Prev.Clear();
			m_Ditry.Clear();
		}
	}

}