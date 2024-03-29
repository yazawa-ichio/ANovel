﻿namespace ANovel.Engine
{
	public class MessageCommandBase : Command
	{
		protected MessageService Service => Get<MessageService>();

		[Argument]
		protected bool m_Sync = true;
		[Argument]
		protected bool m_CanSkip = true;

		protected IPlayHandle m_PlayHandle;

		public override bool IsEnd() => !m_PlayHandle?.IsPlaying ?? true;

		public override bool IsSync() => m_Sync;

		protected override void TryNext()
		{
			if (m_Sync && m_CanSkip)
			{
				m_PlayHandle?.Dispose();
			}
		}

		public override void Finish()
		{
			m_PlayHandle?.Dispose();
		}
	}

	[TagName("message_show")]
	public class MessageShowCommand : MessageCommandBase
	{
		protected override void UpdateEnvData(IEnvData data)
		{
			if (data.TryGetSingle<MessageStatusEnvData>(out var status))
			{
				status.Hide = false;
				data.SetSingle(status);
			}
		}

		protected override void Execute()
		{
			m_PlayHandle = Service.Frame.Show();
		}
	}

	[TagName("message_hide")]
	public class MessageHideCommand : MessageCommandBase
	{
		protected override void UpdateEnvData(IEnvData data)
		{
			data.TryGetSingle<MessageStatusEnvData>(out var status);
			status.Hide = true;
			data.SetSingle(status);
		}

		protected override void Execute()
		{
			m_PlayHandle = Service.Frame.Hide();
		}
	}

}