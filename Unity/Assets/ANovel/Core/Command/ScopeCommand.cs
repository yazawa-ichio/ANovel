using ANovel.Core;

namespace ANovel.Commands
{
	public interface IScopeCommand
	{
		bool AddScope(ICommand command);
	}

	public interface ICanAddScopeCommand
	{
		bool CanAddScope { get; }
	}

	public abstract class ScopeCommand : SystemCommand, IScopeCommand
	{

		bool IScopeCommand.AddScope(ICommand command) => AddScope(command);

		IScopeCommand m_Nest;
		protected virtual bool AddScope(ICommand command)
		{
			if (m_Nest != null)
			{
				if (m_Nest.AddScope(command))
				{
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
			AddScopeImpl(command);
			return false;
		}

		protected abstract void AddScopeImpl(ICommand command);

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

	}


	[TagName("end")]
	[Description("スコープを終了します。汎用コマンドです")]
	public class EndScopeCommand : SystemCommand
	{
	}


}