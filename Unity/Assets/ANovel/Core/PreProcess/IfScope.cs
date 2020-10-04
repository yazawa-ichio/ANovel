using System.Collections.Generic;

namespace ANovel.Core
{
#if ANOVEL_ALLOW_IF_SCOPE
	[TagName("if", LineType.PreProcess)]
#endif
	public class IfScope : PreProcess
	{
		[TagField(Required = true)]
		string m_Condition = default;
		[TagField]
		bool m_Not = default;

		public bool IsCondition(List<string> symbols)
		{
			var ret = symbols.Contains(m_Condition);
			if (m_Not)
			{
				ret = !ret;
			}
			return ret;
		}

	}

#if ANOVEL_ALLOW_IF_SCOPE
	[TagName("elseif", LineType.PreProcess)]
#endif
	public class ElseIfScope : IfScope
	{
	}

#if ANOVEL_ALLOW_IF_SCOPE
	[TagName("else", LineType.PreProcess)]
#endif
	public class ElseScope : PreProcess
	{
	}

#if ANOVEL_ALLOW_IF_SCOPE
	[TagName("endif", LineType.PreProcess)]
#endif
	public class EndIfScope : PreProcess
	{
	}
}