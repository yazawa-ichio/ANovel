namespace ANovel.Minimum
{
	[CommandName("fade_in", Symbol = "ANOVEL_MINIMUM")]
	public class FadeInCommand : MinimumCommand
	{
		[InjectParam]
		FadeConfig m_Config = new FadeConfig();
		[CommandField]
		bool m_Sync = false;
		[CommandField]
		bool m_Skip = true;

		CommandCoroutine m_Process;

		public override bool IsEnd()
		{
			return m_Process.IsEnd;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void Initialize()
		{
			m_Process = Fade.FadeIn(m_Config);
		}

		protected override void Execute()
		{
			m_Process.Run();
		}

		protected override void TryNext()
		{
			if (m_Sync && m_Skip)
			{
				m_Process?.Finish();
			}
		}

		public override void FinishBlock()
		{
			m_Process.Finish();
		}

	}

	[CommandName("fade_out", Symbol = "ANOVEL_MINIMUM")]
	public class FadeOutCommand : MinimumCommand
	{
		[CommandField]
		float m_Time = 0.3f;
		[CommandField]
		bool m_Sync = false;
		[CommandField]
		bool m_Skip = true;

		CommandCoroutine m_Process;

		public override bool IsEnd()
		{
			return m_Process.IsEnd;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void Initialize()
		{
			m_Process = Fade.FadeOut(m_Time);
		}

		protected override void Execute()
		{
			m_Process.Run();
		}

		protected override void TryNext()
		{
			if (m_Sync && m_Skip)
			{
				m_Process?.Finish();
			}
		}

		public override void FinishBlock()
		{
			m_Process.Finish();
		}

	}

}