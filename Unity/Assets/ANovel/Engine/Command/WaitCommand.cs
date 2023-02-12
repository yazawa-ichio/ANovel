namespace ANovel.Engine
{

	[TagName("wait")]
	[Description("指定時間停止します")]
	public class WaitCommand : Command
	{
		IEngineTime Time => Get<IEngineTime>();

		[Argument(Required = true)]
		[Description("指定時間")]
		Millisecond m_Time = default;
		[Argument]
		[Description("スキップ可能か？")]
		protected bool m_CanSkip = false;

		IFadeHandle m_PlayHandle;

		public override bool IsSync() => true;

		public override bool IsEnd() => !m_PlayHandle?.IsPlaying ?? true;

		protected override void Execute()
		{
			m_PlayHandle = new FloatFadeHandle(1, 1f, m_Time.ToSecond());
		}

		public override void Finish()
		{
			m_PlayHandle?.Dispose();
		}

		protected override void TryNext()
		{
			if (m_CanSkip)
			{
				m_PlayHandle?.Dispose();
			}
		}

		protected override void Update()
		{
			m_PlayHandle?.Update(Time.DeltaTime);
		}

	}

	[TagName("wait_click")]
	public class WaitClickCommand : Command
	{
		public override bool IsSync() => true;

		public override bool IsEnd() => m_Clicked;

		bool m_Clicked;

		protected override void Execute()
		{
			m_Clicked = false;
		}

		protected override void TryNext()
		{
			m_Clicked = true;
		}

	}

	[TagName("wait_trig")]
	public class WaitTriggerCommand : Command
	{
		public override bool IsSync() => true;

		public override bool IsEnd() => m_Trigger;

		[Argument(Required = true)]
		string m_Name = null;

		bool m_Trigger;

		protected override void Execute()
		{
			m_Trigger = false;
		}

		[EventSubscribe(EngineEvent.Trigger)]
		void OnTrigger(string name)
		{
			if (m_Name == name)
			{
				m_Trigger = true;
			}
		}

	}


}