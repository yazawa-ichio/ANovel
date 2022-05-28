using ANovel.Core;
using System.Collections.Generic;

namespace ANovel.Commands
{
	[TagName("parallel")]
	[Description("コマンドを並列実行します")]
	public class ParallelCommand : ScopeCommand
	{
		[Argument]
		[Description("同期待ちを行うか？\n空の場合は実行中のコマンドから同期するか判断します")]
		bool? m_Sync = null;

		List<ICommand> m_RunCommands;

		protected override void Initialize()
		{
			m_RunCommands = ListPool<ICommand>.Pop();
		}

		public override void FinishBlock()
		{
			ListPool<ICommand>.Push(m_RunCommands);
			m_RunCommands = null;
		}

		public override bool IsEnd()
		{
			foreach (var cmd in m_RunCommands)
			{
				if (!cmd.IsEnd())
				{
					return false;
				}
			}
			return true;
		}

		public override bool IsSync()
		{
			if (m_Sync.HasValue)
			{
				return m_Sync.Value;
			}
			foreach (var cmd in m_RunCommands)
			{
				if (cmd.IsSync())
				{
					return true;
				}
			}
			return false;
		}

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
			m_RunCommands.Add(command);
		}

	}


}