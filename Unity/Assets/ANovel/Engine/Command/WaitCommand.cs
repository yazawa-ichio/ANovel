﻿namespace ANovel.Commands
{

	[CommandName("wait")]
	public class WaitCommand : Command
	{
		IEngineTime Time => Get<IEngineTime>();

		[CommandField(Required = true)]
		float m_Time = 0;
		[CommandField]
		protected bool m_CanSkip = false;

		IFadeHandle m_PlayHandle;

		public override bool IsSync() => true;

		public override bool IsEnd() => !m_PlayHandle?.IsPlaying ?? true;

		protected override void Execute()
		{
			m_PlayHandle = new FloatFadeHandle(1, 1f, m_Time);
		}

		public override void FinishBlock()
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

	[CommandName("wait_click")]
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

	[CommandName("wait_trig")]
	public class WaitTriggerCommand : Command
	{
		public override bool IsSync() => true;

		public override bool IsEnd() => m_Trigger;

		[CommandField(Required = true)]
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