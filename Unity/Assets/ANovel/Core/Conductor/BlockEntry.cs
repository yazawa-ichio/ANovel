namespace ANovel.Core
{
	public class BlockEntry
	{
		public Block Block { get; private set; }

		public PreLoadScope PreLoad { get; private set; }

		public bool IsPrepared => PreLoad.IsLoaded && !CheckPreparing();

		public bool IsProcess => IsPrepared && (m_SyncCommand != null || m_Index >= Block.Commands.Count);

		public bool IsEndBlock { get; internal set; }

		ICommand m_SyncCommand;
		int m_Index;

		public bool IsProcessing
		{
			get
			{
				var commands = Block.Commands;
				for (int i = 0; i < m_Index; i++)
				{
					var cmd = commands[i];
					if (!cmd.IsEnd())
					{
						return true;
					}
				}
				return false;
			}
		}

		public BlockEntry(Block block, PreLoadScope preLoad)
		{
			Block = block;
			PreLoad = preLoad;
		}

		bool CheckPreparing()
		{
			foreach (var cmd in Block.Commands)
			{
				if (!cmd.IsPrepared())
				{
					return true;
				}
			}
			return false;
		}

		public bool TryProcess()
		{
			if (!IsPrepared)
			{
				return false;
			}
			if (m_SyncCommand != null)
			{
				if (!m_SyncCommand.IsSync() || m_SyncCommand.IsEnd())
				{
					m_SyncCommand = null;
				}
				else
				{
					Update();
					return false;
				}
			}
			var commands = Block.Commands;
			while (m_Index < commands.Count)
			{
				var cmd = commands[m_Index++];
				cmd.Execute();
				if (cmd.IsSync() && !cmd.IsEnd())
				{
					m_SyncCommand = cmd;
					Update();
					return false;
				}
			}
			Update();
			return m_Index == commands.Count;
		}

		public void TryNext()
		{
			var commands = Block.Commands;
			for (int i = 0; i < m_Index; i++)
			{
				var cmd = commands[i];
				if (!cmd.IsEnd())
				{
					cmd.TryNext();
				}
			}
		}

		void Update()
		{
			var commands = Block.Commands;
			for (int i = 0; i < m_Index; i++)
			{
				var cmd = commands[i];
				if (!cmd.IsEnd())
				{
					cmd.Update();
				}
			}
		}

		public void Finish()
		{
			PreLoad?.Dispose();
			foreach (var cmd in Block.Commands)
			{
				cmd.FinishBlock();
			}
		}


	}
}