using ANovel.Core;
using System.Collections.Generic;

namespace ANovel.Commands
{
	public interface IScopeCommand
	{
		void BeginAddCommand();
		void EndAddCommand();
		bool AddCommand(ICommand command);
	}

	public interface ICanAddScopeCommand
	{
		bool CanAddScope { get; }
	}

	public abstract class ScopeCommand : SystemCommand, IScopeCommand
	{
		bool IScopeCommand.AddCommand(ICommand command) => AddCommand(command);
		void IScopeCommand.BeginAddCommand() => BeginAddCommand();
		void IScopeCommand.EndAddCommand() => EndAddCommand();

		IScopeCommand m_Nest;
		List<ICommand> m_Commands;

		protected sealed override void UpdateEnvData(IEnvData data)
		{
			ScopeBeginUpdateEnvData(data);
			foreach (var cmd in m_Commands)
			{
				cmd.Init(Container, Meta, data);
			}
			ScopeEndUpdateEnvData(data);
		}

		protected virtual void ScopeBeginUpdateEnvData(IEnvData data) { }

		protected virtual void ScopeEndUpdateEnvData(IEnvData data) { }

		protected sealed override void Preload(IPreLoader loader)
		{
			PreloadImpl(loader);
			foreach (var cmd in m_Commands)
			{
				cmd.Prepare(loader);
			}
		}

		protected virtual void PreloadImpl(IPreLoader loader) { }

		public sealed override void Finish()
		{
			PreScopeFinish();
			foreach (var cmd in m_Commands)
			{
				cmd.Finish();
			}
			ListPool<ICommand>.Push(m_Commands);
			m_Commands = null;
			ScopeFinish();
		}

		protected virtual void PreScopeFinish() { }

		protected virtual void ScopeFinish() { }

		protected bool AddCommand(ICommand command)
		{
			if (m_Commands == null)
			{
				m_Commands = ListPool<ICommand>.Pop();
			}
			if (m_Nest != null)
			{
				if (m_Nest.AddCommand(command))
				{
					m_Nest.EndAddCommand();
					m_Nest = null;
				}
				return false;
			}
			if (command is IScopeCommand scope)
			{
				if (!CanNest(command))
				{
					throw new LineDataException(LineData, $"can not nest {command}");
				}
				m_Nest = scope;
				m_Nest.BeginAddCommand();
				return false;
			}
			if (IsEndCommand(command))
			{
				return true;
			}
			if (!CanAdd(command))
			{
				throw new LineDataException(LineData, $"can not scope add {command}");
			}
			m_Commands.Add(command);
			OnAddCommand(command);
			return false;
		}

		protected abstract void OnAddCommand(ICommand command);

		protected virtual bool CanNest(ICommand command) => true;

		protected virtual bool CanAdd(ICommand command)
		{
			if (command is ICanAddScopeCommand check)
			{
				return check.CanAddScope;
			}
			if (command is IVariableCommand)
			{
				return false;
			}
			return true;
		}

		protected virtual bool IsEndCommand(ICommand command) => command is EndScopeCommand;

		protected virtual void BeginAddCommand() { }
		protected virtual void EndAddCommand() { }
	}


	[TagName("end")]
	[Description("スコープを終了します。汎用コマンドです")]
	public class EndScopeCommand : SystemCommand
	{
	}


}