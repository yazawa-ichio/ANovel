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
		bool m_DoProcess;
		bool m_ScheduleDispose;

		public BlockEntry(Block block, PreLoadScope preLoad)
		{
			Block = block;
			PreLoad = preLoad;
			m_RunCommands = ListPool<ICommand>.Pop();
		}

		public void Process()
		{
			try
			{
				m_DoProcess = true;
				ProcessImpl();
			}
			finally
			{
				m_DoProcess = false;
				if (m_ScheduleDispose)
				{
					Dispose();
				}
			}

		}

		void ProcessImpl()
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
			if (m_DoProcess)
			{
				m_ScheduleDispose = true;
				return;
			}
			PreLoad?.Dispose();
			foreach (var cmd in Block.Commands)
			{
				cmd.Finish();
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