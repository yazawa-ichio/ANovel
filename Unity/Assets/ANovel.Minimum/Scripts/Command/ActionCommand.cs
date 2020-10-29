namespace ANovel.Minimum
{
	[CommandName("move", Symbol = "ANOVEL_MINIMUM")]
	public class MoveCommand : MinimumCommand
	{
		[InjectParam]
		LayoutConfig m_Config = new LayoutConfig();
		[CommandField]
		public float m_Time = 0.2f;
		[CommandField]
		bool m_Sync = default;
		[CommandField]
		bool m_Skip = true;

		CommandCoroutine m_Command;

		public override bool IsEnd()
		{
			return m_Command.IsEnd;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void Execute()
		{
			m_Command = Image.PlayMoveImage(m_Config, m_Time).Run();
		}

		protected override void TryNext()
		{
			if (m_Sync && m_Skip)
			{
				m_Command?.Finish();
			}
		}

		public override void FinishBlock()
		{
			m_Command?.Finish();
		}

	}

	[CommandName("anim", Symbol = "ANOVEL_MINIMUM")]
	public class AnimCommand : MinimumCommand
	{
		[InjectParam]
		AnimationConfig m_Config = new AnimationConfig();
		[CommandField]
		bool m_Sync = default;
		[CommandField]
		bool m_Skip = true;

		CommandCoroutine m_Command;

		public override bool IsEnd()
		{
			return m_Command.IsEnd;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void Execute()
		{
			m_Command = Image.PlayAnimImage(m_Config).Run();
		}

		protected override void TryNext()
		{
			if (m_Sync && m_Skip)
			{
				m_Command?.Finish();
			}
		}

		public override void FinishBlock()
		{
			m_Command?.Finish();
		}

	}

	[CommandName("stop", Symbol = "ANOVEL_MINIMUM")]
	public class StopActionCommand : MinimumCommand
	{
		[CommandField(Required = true)]
		string m_Name = default;

		protected override void Execute()
		{
			Image.StopActionImage(m_Name);
		}

	}
}