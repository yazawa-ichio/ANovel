using System;
using System.Collections.Generic;

namespace ANovel.Core
{

	public class TagParam
	{

		internal readonly struct ValueEntry
		{
			public readonly string Value;
			public readonly bool UseEvaluator;

			public ValueEntry(string value, bool useEvaluator)
			{
				Value = value;
				UseEvaluator = useEvaluator;
			}
		}

		public LineData Data { get; private set; }

		public string Name
		{
			get => m_Name;
			set
			{
				if (!m_UnlockName) throw new InvalidOperationException("TagParam.Name set TagParam.Set scope allowed only");
				m_Name = value;
			}
		}

		string m_Name;
		bool m_UnlockName;
		internal Dictionary<string, ValueEntry> m_Dic = new Dictionary<string, ValueEntry>();

		public IEvaluator Evaluator { get; set; }
		public int Count => m_Dic.Count;

		public string this[string key]
		{
			get
			{
				return GetValue(key);
			}
		}

		public void AddValue(string key, string value)
		{
			m_Dic[key] = new ValueEntry(value, false);
		}

		public void AddValueWithEvaluator(string key, string value)
		{
			m_Dic[key] = new ValueEntry(value, true);
		}

		public void Set(in LineData data, List<IParamConverter> converters)
		{

			m_Dic.Clear();
			Data = data;
			m_Name = data.ReadName(out var nameEndIndex);
			if (nameEndIndex > 0)
			{
				data.ReadKeyValue(nameEndIndex, this);
			}
			if (converters != null)
			{
				m_UnlockName = true;
				try
				{
					foreach (var converter in converters)
					{
						converter.Convert(this);
					}
				}
				finally
				{
					m_UnlockName = false;
				}
			}
		}


		public string GetValue(string key)
		{
			var ret = m_Dic[key];
			string value;
			if (ret.UseEvaluator)
			{
				value = Evaluator.ReplaceVariable(ret.Value, Data).ToString();
			}
			else
			{
				value = ret.Value;
			}
			return value;
		}

		public bool TryGetValue(string key, out string value)
		{
			if (m_Dic.TryGetValue(key, out var ret))
			{
				if (ret.UseEvaluator)
				{
					value = Evaluator.ReplaceVariable(ret.Value, Data).ToString();
				}
				else
				{
					value = ret.Value;
				}
				return true;
			}
			value = default;
			return false;
		}

		public bool ContainsKey(string key)
		{
			return m_Dic.ContainsKey(key);
		}

		public void TrySetNewValue(string key, string value)
		{
			key = key.ToLower();
			if (!string.IsNullOrEmpty(value) && !m_Dic.ContainsKey(key))
			{
				m_Dic[key] = new ValueEntry(value, useEvaluator: false);
			}
		}

		public void TrySetNewValue<T>(string key, T? value) where T : struct
		{
			key = key.ToLower();
			if (value.HasValue && !m_Dic.ContainsKey(key))
			{
				m_Dic[key] = new ValueEntry(value.ToString(), useEvaluator: false);
			}
		}

		Dictionary<string, string> m_ToDictionaryCache;
		public Dictionary<string, string> ToDictionary()
		{
			if (m_ToDictionaryCache == null)
			{
				m_ToDictionaryCache = new Dictionary<string, string>(16);
			}
			m_ToDictionaryCache.Clear();
			var ret = m_ToDictionaryCache;
			foreach (var key in m_Dic.Keys)
			{
				ret[key] = GetValue(key);
			}
			return ret;
		}

	}

}