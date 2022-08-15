using ANovel.Core;
using System.Collections.Generic;

namespace ANovel.Commands
{
	[TagName("async")]
	[Description("スコープを非同期実行します")]
	public class AsyncScopeCommand : ScopeCommand
	{
		List<ICommand> m_BatchCommands;
		List<ICommand> m_RunCommands;
		ICommand m_SyncCommand;

		protected override void Prepare()
		{
			m_BatchCommands = ListPool<ICommand>.Pop();
			m_RunCommands = ListPool<ICommand>.Pop();
		}

		public override void Finish()
		{
			foreach (var cmd in m_BatchCommands)
			{
				cmd.Execute();
			}
			ListPool<ICommand>.Push(m_BatchCommands);
			ListPool<ICommand>.Push(m_RunCommands);
			m_BatchCommands = null;
			m_RunCommands = null;
		}

		public override bool IsEnd()
		{
			if (m_BatchCommands.Count > 0)
			{
				return false;
			}
			foreach (var cmd in m_RunCommands)
			{
				if (!cmd.IsEnd())
				{
					return false;
				}
			}
			return true;
		}

		public override bool IsSync() => false;

		protected override void Execute()
		{
			foreach (var cmd in m_RunCommands)
			{
				cmd.Execute();
			}
		}

		protected override void TryNext()
		{
			foreach (var cmd in m_RunCommands)
			{
				if (!cmd.IsEnd())
				{
					cmd.TryNext();
				}
			}
		}

		protected override void Update()
		{
			if (m_SyncCommand != null)
			{
				if (!m_SyncCommand.IsSync() || m_SyncCommand.IsEnd())
				{
					m_SyncCommand = null;
				}
			}
			else
			{
				while (m_BatchCommands.Count > 0)
				{
					var cmd = m_BatchCommands[0];
					m_BatchCommands.RemoveAt(0);
					m_RunCommands.Add(cmd);
					cmd.Execute();
					if (cmd.IsSync() && !cmd.IsEnd())
					{
						m_SyncCommand = cmd;
						break;
					}
				}
			}
			foreach (var cmd in m_RunCommands)
			{
				if (!cmd.IsEnd())
				{
					cmd.Update();
				}
			}
		}

		protected override void AddScopeImpl(ICommand command)
		{
			m_BatchCommands.Add(command);
		}

	}

}