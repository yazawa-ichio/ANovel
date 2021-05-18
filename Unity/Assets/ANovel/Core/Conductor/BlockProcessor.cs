using ANovel.Commands;
using System.Collections.Generic;

namespace ANovel.Core
{
	public class BlockProcessor
	{
		enum State
		{
			StartNext,
			WaitPreload,
			ProcessCommand,
			ProcessText,
			Finished,
		}

		public History History { get; private set; } = new History();

		public BlockLabelInfo CurrentLabel => History.CurrentLabel;

		public int PreloadCount { get; set; } = 3;

		public bool CanPreload => m_PreloadQueue.Count == 0 || m_PreloadQueue.Count < PreloadCount;

		public ServiceContainer Container { get; private set; }

		public ResourceCache Cache { get; private set; }

		public ITextProcessor Text { get; set; }

		public IEnvDataHolder Current => m_CurrentEnvData;

		public EnvDataHook EnvDataHook { get; private set; } = new EnvDataHook();

		public bool IsStop => m_StopCommand != null || (m_PreloadQueue.Count == 0 && m_CurrentBlock == null);

		public bool IsWaitNext
		{
			get
			{
				if (m_State == State.ProcessText)
				{
					return !Text?.IsProcessing ?? true;
				}
				return m_State == State.Finished;
			}
		}

		State m_State = State.StartNext;
		Queue<BlockPreloadEntry> m_PreloadQueue = new Queue<BlockPreloadEntry>();
		EnvData m_CurrentEnvData = new EnvData();
		EnvData m_PreUpdateEnvData = new EnvData();
		IStopCommand m_StopCommand;
		BlockEntry m_CurrentBlock;

		public BlockProcessor(ServiceContainer container, ResourceCache cache)
		{
			Container = container;
			Cache = cache;
			Container.Set<IHistory>(History);
		}

		public void Reset()
		{
			ClearPreload();
			m_CurrentEnvData.Clear();
			m_PreUpdateEnvData.Clear();
			History.Clear();
			Text?.Clear();
			m_State = State.StartNext;
		}

		public void Dispose()
		{
			ClearPreload();
		}

		public void ClearPreload()
		{
			if (m_CurrentBlock != null)
			{
				m_CurrentBlock?.Dispose();
				m_CurrentBlock = null;
			}
			while (m_PreloadQueue.Count > 0)
			{
				m_PreloadQueue.Dequeue().PreLoad.Dispose();
			}
		}

		public void PostJump(PreProcessor.Result result)
		{
			m_StopCommand = null;
			ClearPreload();
			EnvDataHook.PostJump(result.Meta, m_PreUpdateEnvData);
			m_State = State.StartNext;
		}

		public void UpdateCurrent(Block block)
		{
			EnvDataHook.PreUpdate(m_CurrentEnvData, block);
			foreach (var cmd in block.Commands)
			{
				cmd.SetMetaData(block.Meta);
				cmd.UpdateEnvData(m_CurrentEnvData);
			}
			EnvDataHook.PostUpdate(m_CurrentEnvData, block);
			History.Add(m_CurrentEnvData, block, m_CurrentEnvData.Diff());
		}

		public void PostSeek(Block block)
		{
			m_PreUpdateEnvData.Load(m_CurrentEnvData.Save());
			m_StopCommand = null;
			ClearPreload();
			AddPreloadBlock(block);
			if (block.ClearCurrentText)
			{
				Text?.Clear();
			}
			m_State = State.WaitPreload;
			//Process();
		}

		public StoreData Back(int num)
		{
			if (!History.TryBack(num, out var diff))
			{
				return null;
			}
			foreach (var d in diff)
			{
				m_CurrentEnvData.Undo(d);
			}
			var snapshot = m_CurrentEnvData.Save();
			m_PreUpdateEnvData.Load(snapshot);
			var data = new StoreData
			{
				Label = History.CurrentLabel,
				Snapshot = snapshot,
				Logs = History.Save(),
			};
			return data;
		}

		public void AddPreloadBlock(Block block)
		{
			EnvDataHook.PreUpdate(m_PreUpdateEnvData, block);
			foreach (var cmd in block.Commands)
			{
				cmd.SetMetaData(block.Meta);
				cmd.UpdateEnvData(m_PreUpdateEnvData);
			}
			EnvDataHook.PostUpdate(m_PreUpdateEnvData, block);
			var diff = !block.SkipHistory ? m_PreUpdateEnvData.Diff() : new EnvDataDiff();
			var container = Container.CreateChild();
			var scope = new PreLoadScope(Cache);
			container.Set<IPreLoader>(scope);
			foreach (var cmd in block.Commands)
			{
				cmd.Initialize(container);
			}
			m_PreloadQueue.Enqueue(new BlockPreloadEntry(block, scope, diff));
		}

		bool TryStartNext()
		{
			if (m_PreloadQueue.Count == 0 || !m_PreloadQueue.Peek().IsDone)
			{
				return false;
			}
			var preload = m_PreloadQueue.Dequeue();
			preload.CheckError();
			m_CurrentBlock = new BlockEntry(preload.Block, preload.PreLoad);
			m_StopCommand = m_CurrentBlock.Block.StopCommand;
			if (m_StopCommand == null || m_StopCommand.ClearText)
			{
				Text?.Clear();
			}
			m_CurrentEnvData.Redo(preload.Diff);
			History.Add(m_CurrentEnvData, preload.Block, preload.Diff);
			return true;
		}

		public void Process()
		{
			if (m_State == State.StartNext || m_State == State.WaitPreload)
			{
				if (!TryStartNext())
				{
					m_State = State.WaitPreload;
				}
				else
				{
					m_State = State.ProcessCommand;
				}
			}
			m_CurrentBlock?.Process();
			if (m_State == State.ProcessCommand && !m_CurrentBlock.HasPendingCommand)
			{
				if (Text != null && m_CurrentBlock.Block.Text != null)
				{
					m_State = State.ProcessText;
					Text.Set(m_CurrentBlock.Block.Text, m_CurrentEnvData);
				}
				else
				{
					m_State = State.Finished;
				}
			}
		}

		public bool TryNext()
		{
			if (m_State == State.ProcessCommand)
			{
				m_CurrentBlock?.TryNext();
				return false;
			}
			if (m_State == State.ProcessText)
			{
				if (Text.IsProcessing && !Text.TryNext())
				{
					return false;
				}
				m_State = State.Finished;
			}
			if (m_State != State.Finished)
			{
				return false;
			}
			m_State = State.StartNext;
			if (m_CurrentBlock != null)
			{
				m_CurrentBlock.Dispose();
				m_CurrentBlock = null;
			}
			return true;
		}

		public StoreData Store()
		{
			return new StoreData
			{
				Label = History.CurrentLabel,
				Snapshot = m_CurrentEnvData.Save(),
				Logs = History.Save(),
			};
		}

		public void Restore(StoreData data, Block block)
		{
			m_StopCommand = block.StopCommand;
			m_CurrentEnvData.Load(data.Snapshot);
			m_PreUpdateEnvData.Load(data.Snapshot);
			History.Load(data.Logs);
			m_State = State.Finished;
		}

	}

}