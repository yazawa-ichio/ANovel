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
		[Argument]
		string m_Flag;
		[Argument(KeyName = "has_val")]
		string m_HasVal;

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
				if (!string.IsNullOrEmpty(m_Flag))
				{
					if (evaluator.Variables.TryGetValue(m_Flag, out var val) && val is bool ret)
					{
						return ret;
					}
					if (evaluator.GlobalVariables.TryGetValue(m_Flag, out val) && val is bool glocalRet)
					{
						return glocalRet;
					}
					return false;
				}
				if (!string.IsNullOrEmpty(m_HasVal))
				{
					return evaluator.Variables.Has(m_HasVal) || evaluator.GlobalVariables.Has(m_HasVal);
				}
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


	[TagName("elseif")]
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