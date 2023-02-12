using System.Collections.Generic;

namespace ANovel.Core
{
	[TagName("if")]
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

	[TagName("elseif")]
	public class ElseIfScope : IfScope
	{
		public override bool HeaderOnly => false;
	}

	[TagName("else")]
	public class ElseScope : PreProcess
	{
		public override bool HeaderOnly => false;
	}

	[TagName("endif")]
	public class EndIfScope : PreProcess
	{
		public override bool HeaderOnly => false;
	}
}