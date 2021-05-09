using ANovel.Core;
using ANovel.Service;

namespace ANovel.Commands
{
	public class MessageCommandBase : Command
	{
		protected MessageService Service => Get<MessageService>();

		[CommandField]
		protected bool m_Sync = true;
		[CommandField]
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

		public override void FinishBlock()
		{
			m_PlayHandle?.Dispose();
		}
	}

	[CommandName("message_show")]
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

	[CommandName("message_hide")]
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