using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{


	public interface IBranchCommand
	{
		bool IsStartScope { get; }
		LineData LineData { get; }
		bool Branch(IEvaluator evaluator);
	}

	public interface IBranchEnd
	{
		LineData LineData { get; }
	}

	public enum BranchState
	{
		Start,
		Done,
		End,
	}

	public struct BranchEnvData : IEnvValue
	{
		public BranchState[] StackDataState;
	}

	public class BranchController
	{
		static string[] s_BranchTag = new string[]
		{
			"if",
			"elsif",
			"else",
			"endif",
		};

		class StackData
		{
			public BranchState State;
		}

		Stack<StackData> m_Stack = new Stack<StackData>();

		public BranchState BranchState => m_Stack.Count > 0 ? m_Stack.Peek().State : BranchState.Done;

		public bool IsSkip => BranchState != BranchState.Done;

		public bool IsIfScope => m_Stack.Count > 0;

		public bool CheckIgnore(TagParam param)
		{
			if (m_Stack.Count == 0)
			{
				return false;
			}
			switch (m_Stack.Peek().State)
			{
				case BranchState.Done:
					return false;
			}
			if (Array.IndexOf(s_BranchTag, param.Name) >= 0)
			{
				return false;
			}
			return true;
		}

		public void CheckFinish(LineData data)
		{
			if (m_Stack.Count != 0)
			{
				throw new LineDataException(in data, $"if scope not end. stack count {m_Stack.Count}");
			}
		}

		public void BranchCommand(IEvaluator evaluator, IBranchCommand command)
		{
			switch (BranchState)
			{
				case BranchState.Done:
					if (command.IsStartScope)
					{
						m_Stack.Push(new StackData { State = BranchState.Start });
						if (command.Branch(evaluator))
						{
							m_Stack.Peek().State = BranchState.Done;
						}
					}
					else if (m_Stack.Count == 0)
					{
						throw new LineDataException(command.LineData, "");
					}
					else
					{
						m_Stack.Peek().State = BranchState.End;
					}
					break;
				case BranchState.Start:
					if (command.IsStartScope)
					{
						m_Stack.Push(new StackData { State = BranchState.End });
					}
					else if (command.Branch(evaluator))
					{
						m_Stack.Peek().State = BranchState.Done;
					}
					break;
				case BranchState.End:
					if (command.IsStartScope)
					{
						m_Stack.Push(new StackData { State = BranchState.End });
					}
					break;
			}
		}

		public void BranchEnd(IBranchEnd command)
		{
			if (m_Stack.Count == 0)
			{
				throw new LineDataException(command.LineData, "not found branch start command.");
			}
			m_Stack.Pop();
		}

		public void Clear()
		{
			m_Stack.Clear();
		}

		public void Save(IEnvData data)
		{
			data.SetSingle(new BranchEnvData
			{
				StackDataState = m_Stack.Select(x => x.State).ToArray(),
			});
		}

		public void Load(IEnvData data)
		{
			m_Stack.Clear();
			if (data.TryGetSingle(out BranchEnvData ret))
			{
				foreach (var state in ret.StackDataState)
				{
					m_Stack.Push(new StackData
					{
						State = state,
					});
				}
			}
		}

	}
}