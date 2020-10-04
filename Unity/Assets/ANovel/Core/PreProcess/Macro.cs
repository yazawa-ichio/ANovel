using System;
using System.Collections.Generic;

namespace ANovel.Core
{

	public class Macro
	{
		MacroDefine m_Owner;
		LineData[] m_Line;

		TagParam m_Param = new TagParam();
		List<string> m_ParamKeys = new List<string>();
		List<string> m_ParamValues = new List<string>();
		bool m_Do;

		public Macro(MacroDefine owner, LineData[] line)
		{
			m_Owner = owner;
			m_Line = line;
		}

		public void Provide(List<string> symbols, Dictionary<string, string> variables, List<Tag> ret)
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

		void Assign(in LineData data, Dictionary<string, string> variables)
		{
			m_Param.Set(in data, m_Owner.m_Converters);
			foreach (var kvp in m_Param)
			{
				m_ParamKeys.Add(kvp.Key);
				m_ParamValues.Add(kvp.Value);
			}
			for (int i = 0; i < m_ParamValues.Count; i++)
			{
				var val = m_ParamValues[i];
				if (val == null || val.Length < 3)
				{
					continue;
				}
				if (val[0] == '%' && val[val.Length - 1] == '%')
				{
					val = val.Substring(1, val.Length - 2);
					if (variables.TryGetValue(val, out var replace))
					{
						m_Param[m_ParamKeys[i]] = replace;
					}
					else
					{
						m_Param[m_ParamKeys[i]] = null;
					}
				}
			}
		}

	}
}