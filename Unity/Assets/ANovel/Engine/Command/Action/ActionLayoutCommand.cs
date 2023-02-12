using ANovel.Actions;

namespace ANovel.Engine
{

	[TagName("action_layout")]
	public class ActionLayoutCommand : ActionParamCommand<ImageObjectActionCommandBase>
	{
		[InjectArgument]
		public PlayAnimConfig Config = new PlayAnimConfig();
		[InjectArgument(IgnoreKey = "level")]
		public LayoutConfig Layout = new LayoutConfig();
		[Argument]
		public bool Sync;

		protected override void AddParamImpl(ImageObjectActionCommandBase owner, ActionParamContext context)
		{
			var prev = LayoutConfig.Restore(owner.TargetName, context.EnvData);
			var to = Layout.Clone();
			LayoutConfig.UpdateEvnData(owner.TargetName, context.EnvData, Layout);
			context.Add(new ActionLayoutEnvData
			{
				Config = Config,
				From = prev,
				To = to,
				Start = context.Time,
			}, Sync);
		}
	}

}