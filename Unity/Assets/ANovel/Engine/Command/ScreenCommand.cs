using ANovel.Core;
using ANovel.Service;
using System.Collections.Generic;
using UnityEngine;

namespace ANovel.Commands
{
	public interface IUseTransitionScope
	{
		bool Use { get; }
	}

	[SystemCommandName("transition")]
	public class TransitionCommand : ScopeCommand
	{
		protected PathConfig Path => Get<EngineConfig>().Path;

		protected IScreenService Service => Get<IScreenService>();

		[CommandField]
		bool m_Sync = false;
		[CommandField]
		bool m_CanSkip = true;
		[InjectParam]
		ScreenTransitionConfig m_Config = new ScreenTransitionConfig();
		[CommandField]
		string m_Rule = default;

		IPlayHandle m_PlayHandle;

		List<ICommand> m_RunCommands;

		protected override bool CanAdd(ICommand command)
		{
			return command is IUseTransitionScope target && target.Use;
		}

		protected override bool CanNest(ICommand command)
		{
			return false;
		}

		protected override void Initialize()
		{
			m_RunCommands = ListPool<ICommand>.Pop();
		}

		public override void FinishBlock()
		{
			ListPool<ICommand>.Push(m_RunCommands);
			m_RunCommands = null;
			m_PlayHandle?.Dispose();
		}

		protected override void AddScopeImpl(ICommand command)
		{
			m_RunCommands.Add(command);
		}

		public override bool IsEnd()
		{
			return !m_PlayHandle?.IsPlaying ?? true;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void UpdateEnvData(IEnvData data)
		{
			if (!m_Config.Copy)
			{
				data.DeleteAllByInterface<IScreenChildEnvData>();
			}
		}

		protected override void Preload(IPreLoader loader)
		{
			if (!string.IsNullOrEmpty(m_Rule))
			{
				loader.Load<Texture>(Path.GetRule(m_Rule));
			}
		}

		protected override void Execute()
		{
			if (!string.IsNullOrEmpty(m_Rule))
			{
				m_Config.Rule = Cache.Load<Texture>(Path.GetRule(m_Rule));
			}
			Service.Transition.Prepare(m_Config);
			foreach (var cmd in m_RunCommands)
			{
				cmd.Execute();
				Debug.Assert(cmd.IsEnd(), cmd);
			}
			m_PlayHandle = Service.Transition.Start();
		}

		protected override void TryNext()
		{
			if (IsSync() && m_CanSkip)
			{
				m_PlayHandle.Dispose();
			}
		}

	}

}