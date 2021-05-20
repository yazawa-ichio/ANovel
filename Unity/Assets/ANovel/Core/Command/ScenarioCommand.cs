namespace ANovel.Commands
{

	public enum ScenarioEvent
	{
		Jump,
		Stop,
	}

	public class ScenarioJumpEvent
	{
		public string Path = null;
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
	public class StopCommand : SystemCommand, IStopCommand, ICanAddScopeCommand, ISkipHistoryCommand
	{
		[Argument]
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
	public class ScenarioJumpCommand : SystemCommand, IStopCommand, ISkipHistoryCommand, ICanAddScopeCommand
	{
		[Argument]
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
	public class EndBlockCommand : SystemCommand, ISkipHistoryCommand, ICanAddScopeCommand, IEndBlockCommand
	{
		[Argument(KeyName = "cansave")]
		bool m_CanSave = true;

		public bool SkipHistory => !m_CanSave;

		public bool CanAddScope => false;

	}

}