using ANovel.Core;

namespace ANovel.Commands
{

	internal abstract class ConditionCommand : SystemCommand, IBranchCommand
	{
		[Argument]
		string m_Condition;

		public abstract bool IsStartScope { get; }

		public bool Branch(IEvaluator evaluator)
		{
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