using ANovel.Core;

namespace ANovel.Commands
{

	internal abstract class ConditionCommand : SystemCommand, IBranchCommand
	{
		[Argument]
		string m_Condition;
		[Argument]
		string m_Left;
		[Argument]
		string m_Right;
		[Argument]
		bool m_Not;

		public abstract bool IsStartScope { get; }

		public bool Branch(IEvaluator evaluator)
		{
			var ret = BranchImpl(evaluator);
			if (m_Not)
			{
				return !ret;
			}
			else
			{
				return ret;
			}
		}

		bool BranchImpl(IEvaluator evaluator)
		{
			if (string.IsNullOrEmpty(m_Condition))
			{
				if (bool.TryParse(m_Left, out var left) && bool.TryParse(m_Right, out var right))
				{
					return left == right;
				}
				return m_Left == m_Right;
			}
			return evaluator.Condition(m_Condition, LineData);
		}
	}

	[TagName("if")]
	internal class IfCommand : ConditionCommand
	{
		public override bool IsStartScope => true;
	}


	[TagName("elsif")]
	internal class ElseIfCommand : ConditionCommand
	{
		public override bool IsStartScope => false;
	}

	[TagName("else")]
	internal class ElseCommand : SystemCommand, IBranchCommand
	{
		public bool IsStartScope => false;

		public bool Branch(IEvaluator evaluator)
		{
			return true;
		}
	}

	[TagName("endif")]
	internal class EndIfCommand : SystemCommand, IBranchEnd
	{
	}
}