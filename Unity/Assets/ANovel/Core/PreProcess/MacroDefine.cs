﻿using System.Collections.Generic;

namespace ANovel.Core
{
	public class MacroDefine
	{
		Dictionary<string, Macro> m_Macro = new Dictionary<string, Macro>();
		internal List<IParamConverter> m_Converters;
		List<MacroDefine> m_Depends;
		HashSet<MacroDefine> m_Check = new HashSet<MacroDefine>();

		public MacroDefine(List<IParamConverter> converters, List<MacroDefine> depends)
		{
			m_Converters = converters;
			m_Depends = depends;
		}

		public void Add(string name, LineData[] line)
		{
			m_Macro[name] = new Macro(this, line);
		}

		public bool TryProvide(List<string> symbols, TagParam param, out IEnumerable<Tag> ret)
		{
			try
			{
				m_Check.Add(this);
				return TryProvideImpl(symbols, param, out ret, m_Check);
			}
			finally
			{
				m_Check.Clear();
			}
		}

		bool TryProvideImpl(List<string> symbols, TagParam param, out IEnumerable<Tag> ret, HashSet<MacroDefine> check)
		{
			if (m_Macro.TryGetValue(param.Name, out var macro))
			{
				ret = macro.Provide(symbols, param);
				return true;
			}
			if (m_Depends != null)
			{
				foreach (var dep in m_Depends)
				{
					if (check.Add(dep) && dep.TryProvideImpl(symbols, param, out ret, check))
					{
						return true;
					}
				}
			}
			ret = default;
			return false;
		}
	}
}