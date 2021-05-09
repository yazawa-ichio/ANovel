namespace ANovel.Commands
{
	public class SyncCommandBase : Command
	{
		protected PathConfig Path => Get<EngineConfig>().Path;

		protected IEngineTime Time => Get<IEngineTime>();

		[CommandField]
		protected bool m_Sync = false;
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

}