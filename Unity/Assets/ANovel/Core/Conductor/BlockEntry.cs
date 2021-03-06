using ANovel.Commands;
using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public class BlockEntry : IDisposable
	{
		public Block Block { get; private set; }

		public PreLoadScope PreLoad { get; private set; }

		public bool HasPendingCommand => m_SyncCommand != null || m_Index < Block.Commands.Count;

		public bool IsProcessing
		{
			get
			{
				foreach (var cmd in m_RunCommands)
				{
					if (!cmd.IsEnd())
					{
						return true;
					}
				}
				return false;
			}
		}

		ICommand m_SyncCommand;
		int m_Index;
		List<ICommand> m_RunCommands;

		public BlockEntry(Block block, PreLoadScope preLoad)
		{
			Block = block;
			PreLoad = preLoad;
			m_RunCommands = ListPool<ICommand>.Pop();
		}

		public void Process()
		{
			if (m_SyncCommand != null)
			{
				if (!m_SyncCommand.IsSync() || m_SyncCommand.IsEnd())
				{
					m_SyncCommand = null;
				}
				else
				{
					CommandUpdate();
					return;
				}
			}
			var commands = Block.Commands;
			while (m_Index < commands.Count)
			{
				var cmd = commands[m_Index++];
				if (cmd is IScopeCommand batch)
				{
					bool batchEnd = false;
					while (m_Index < commands.Count)
					{
						if (batch.AddScope(commands[m_Index++]))
						{
							batchEnd = true;
							break;
						}
					}
					if (!batchEnd)
					{
						throw new Exception("not found batch end");
					}
				}
				m_RunCommands.Add(cmd);
				cmd.Execute();
				if (cmd.IsSync() && !cmd.IsEnd())
				{
					m_SyncCommand = cmd;
					CommandUpdate();
					return;
				}
			}
			CommandUpdate();
		}

		public void TryNext()
		{
			foreach (var cmd in m_RunCommands)
			{
				if (!cmd.IsEnd())
				{
					cmd.TryNext();
				}
			}
		}

		void CommandUpdate()
		{
			foreach (var cmd in m_RunCommands)
			{
				if (!cmd.IsEnd())
				{
					cmd.Update();
				}
			}
		}

		public void Dispose()
		{
			PreLoad?.Dispose();
			foreach (var cmd in Block.Commands)
			{
				cmd.FinishBlock();
			}
			if (m_RunCommands != null)
			{
				ListPool<ICommand>.Push(m_RunCommands);
				m_RunCommands = null;
			}
			Block?.Dispose();
		}


	}
}