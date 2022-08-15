using ANovel.Commands;
using ANovel.Core;
using System.Collections.Generic;
using UnityEngine;

namespace ANovel.Engine
{
	public interface IUseTransitionScope
	{
		bool Use { get; }
	}

	[TagName("transition")]
	[Description("画面切り替えを行います")]
	public class TransitionCommand : ScopeCommand
	{
		protected PathConfig Path => Get<EngineConfig>().Path;

		protected IScreenService Service => Get<IScreenService>();

		[Argument]
		[Description("実行を同期するか？")]
		bool m_Sync = false;
		[Argument]
		[Description("スキップ可能か？")]
		bool m_CanSkip = true;
		[InjectArgument]
		ScreenTransitionConfig m_Config = new ScreenTransitionConfig();
		[Argument]
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

		protected override void Prepare()
		{
			m_RunCommands = ListPool<ICommand>.Pop();
		}

		public override void Finish()
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
				loader.Load<Texture>(Path.GetPath(PathCategory.Rule, m_Rule));
			}
		}

		protected override void Execute()
		{
			if (!string.IsNullOrEmpty(m_Rule))
			{
				m_Config.Rule = Cache.Load<Texture>(Path.GetPath(PathCategory.Rule, m_Rule));
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