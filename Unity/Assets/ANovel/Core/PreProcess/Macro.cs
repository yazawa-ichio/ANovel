using System;
using System.Collections.Generic;
using System.Text;

namespace ANovel.Core
{

	public class Macro
	{
		MacroDefine m_Owner;
		LineData[] m_Line;

		TagParam m_Param = new TagParam();
		List<string> m_ParamKeys = new List<string>();
		List<TagParam.ValueEntry> m_ParamValues = new List<TagParam.ValueEntry>();
		bool m_Do;

		public Macro(MacroDefine owner, LineData[] line)
		{
			m_Owner = owner;
			m_Line = line;
		}

		public void Provide(List<string> symbols, TagParam variables, List<Tag> ret)
		{
			if (m_Do) throw new InvalidOperationException("recursive call to macro is not allowed");
			try
			{
				m_Do = true;
				foreach (var data in m_Line)
				{
					Assign(in data, variables);
					if (m_Owner.TryProvide(symbols, m_Param, ret))
					{
						continue;
					}
					else
					{
						if (TagEntry.TryGet(in data, m_Param.Name, symbols, out var entry))
						{
							ret.Add(entry.Create(in data, m_Param));
						}
					}
				}
			}
			finally
			{
				m_Do = false;
			}
		}


		void Assign(in LineData data, TagParam variables)
		{
			m_Param.Evaluator = variables.Evaluator;
			m_Param.Set(in data, m_Owner.m_Converters);

			foreach (var kvp in m_Param.m_Dic)
			{
				m_ParamKeys.Add(kvp.Key);
				m_ParamValues.Add(kvp.Value);
			}
			for (int i = 0; i < m_ParamValues.Count; i++)
			{
				var val = m_ParamValues[i];
				if (val.Value == null || !val.Value.Contains("%"))
				{
					continue;
				}
				var ret = Replace(val.Value, variables);
				if (string.IsNullOrEmpty(ret))
				{
					ret = null;
				}
				if (val.UseEvaluator)
				{
					m_Param.AddValueWithEvaluator(m_ParamKeys[i], ret);
				}
				else
				{
					m_Param.AddValue(m_ParamKeys[i], ret);
				}
			}
		}

		StringBuilder m_Body = new StringBuilder();
		StringBuilder m_Key = new StringBuilder();
		string Replace(string value, TagParam variables)
		{
			var ret = m_Body.Clear();
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				if (c == '%')
				{
					i++;
					if (i < value.Length && value[i] == '%')
					{
						ret.Append(c);
						continue;
					}
					ret.Append(GetValue(ref i, value, variables));
				}
				else
				{
					ret.Append(c);
				}
			}
			return ret.ToString();
		}

		string GetValue(ref int i, string path, TagParam variables)
		{
			var key = m_Key.Clear();
			bool end = false;
			while (i < path.Length)
			{
				var c = path[i];
				if (c == '%')
				{
					end = true;
					break;
				}
				key.Append(c);
				i++;
			}
			if (variables.TryGetValue(key.ToString(), out var ret))
			{
				return ret;
			}
			if (end)
			{
				return "";
			}
			return key.ToString();
		}

	}
}