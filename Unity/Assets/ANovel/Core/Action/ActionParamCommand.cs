namespace ANovel.Actions
{

	public interface IActionParamCommand
	{
		bool IsTarget(object target);
		void AddParam(object target, ActionParamContext context);
	}

	public abstract class ActionParamCommand : Command, IActionParamCommand
	{
		public virtual bool IsTarget(object target) => true;
		public abstract void AddParam(object target, ActionParamContext context);
	}

	public abstract class ActionParamCommand<T> : ActionParamCommand
	{
		public override bool IsTarget(object target) => target is T;
		public override void AddParam(object target, ActionParamContext context)
		{
			AddParamImpl((T)target, context);
		}
		protected abstract void AddParamImpl(T target, ActionParamContext context);
	}

}