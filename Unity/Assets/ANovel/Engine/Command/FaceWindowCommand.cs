namespace ANovel.Engine
{
	[TagName("face_window_show")]
	public class FaceWindowShowCommand : Command
	{
		PathConfig Path => Get<EngineConfig>().Path;

		[InjectArgument]
		public FaceWindowConfig Config = new FaceWindowConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data.UpdateSingle<FaceWindowEnvData, FaceWindowConfig>(Config);
		}

		protected override void Preload(IPreLoader loader)
		{
			Config.PreloadTexture(Path.CharaFaceWindowRoot, loader);
		}

		protected override void Execute()
		{
			Config.LoadTexture(Path.CharaFaceWindowRoot, Cache);
			Event.Publish(FaceWindowEvent.Show, Config);
		}

	}

	[TagName("face_window_hide")]
	public class FaceWindowHideCommand : Command
	{
		protected override void UpdateEnvData(IEnvData data)
		{
			data.DeleteSingle<FaceWindowEnvData>();
		}

		protected override void Execute()
		{
			Event.Publish(FaceWindowEvent.Hide);
		}
	}
}