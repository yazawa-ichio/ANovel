namespace ANovel.Actions
{
	[TagName("action_wait")]
	public class ActionWaitCommand : ActionParamCommand
	{
		[Argument(Required = true)]
		[Description("指定時間")]
		Millisecond m_Time = default;

		public override void AddParam(object target, ActionParamContext context)
		{
			context.Wait(m_Time);
		}
	}
}