namespace ANovel.Commands
{

	public enum ScenarioEvent
	{
		Jump,
		Stop,
	}

	public class ScenarioJumpEvent
	{
		[Description("遷移先のファイルパスです")]
		public string Path = null;
		[Description("遷移先のラベルです")]
		public string Label = null;
	}

	public interface IEndBlockCommand { }

	public interface IStopCommand : IEndBlockCommand
	{
		bool ClearText { get; }
	}

	public interface ISkipHistoryCommand
	{
		bool SkipHistory { get; }
	}

	[TagName("stop")]
	[Description("シナリオの読み込みを停止します")]
	public class StopCommand : SystemCommand, IStopCommand, ICanAddScopeCommand, ISkipHistoryCommand
	{
		[Argument]
		[Description("テキストをクリアします")]
		bool m_ClearText = false;

		public bool ClearText => m_ClearText;

		public bool SkipHistory => true;

		public bool CanAddScope => false;

		protected override void Execute()
		{
			Event.Publish(ScenarioEvent.Stop);
		}

	}

	[TagName("jump")]
	[Description("シナリオをジャンプします")]
	public class ScenarioJumpCommand : SystemCommand, IStopCommand, ISkipHistoryCommand, ICanAddScopeCommand
	{
		[Argument]
		[Description("テキストをクリアします")]
		bool m_ClearText = true;
		[InjectArgument]
		ScenarioJumpEvent m_JumpEvent = new ScenarioJumpEvent();

		public bool ClearText => m_ClearText;

		public bool SkipHistory => true;

		public bool CanAddScope => false;

		protected override void Execute()
		{
			if (string.IsNullOrEmpty(m_JumpEvent.Path))
			{
				m_JumpEvent.Path = LineData.FileName;
			}
			Event.Publish(ScenarioEvent.Jump, m_JumpEvent);
		}

	}

	[TagName("endblock")]
	[Description("ブロックを終了します")]
	public class EndBlockCommand : SystemCommand, ISkipHistoryCommand, ICanAddScopeCommand, IEndBlockCommand
	{
		[Argument]
		[Description("セーブ可能にするか？")]
		bool m_CanSave = true;

		public bool SkipHistory => !m_CanSave;

		public bool CanAddScope => false;

	}

}