using System.Collections.Generic;

namespace ANovel.Core
{
#if ANOVEL_ALLOW_IF_SCOPE
	[TagName("if")]
#endif
	public class IfScope : PreProcess
	{
		public override bool HeaderOnly => false;

		[Argument(Required = true)]
		string m_Condition = default;
		[Argument]
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
	[TagName("elseif")]
#endif
	public class ElseIfScope : IfScope
	{
		public override bool HeaderOnly => false;
	}

#if ANOVEL_ALLOW_IF_SCOPE
	[TagName("else")]
#endif
	public class ElseScope : PreProcess
	{
		public override bool HeaderOnly => false;
	}

#if ANOVEL_ALLOW_IF_SCOPE
	[TagName("endif")]
#endif
	public class EndIfScope : PreProcess
	{
		public override bool HeaderOnly => false;
	}
}