using ANovel.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core
{
	internal class BlockProcessor
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

		public IResourceCache Cache { get; private set; }

		public ITextProcessor Text { get; set; }

		public IEnvDataHolder Current => m_CurrentEnvData;

		public EnvDataHook EnvDataHook { get; private set; }

		public bool IsStop => !m_Reader.CanRead && (m_StopCommand != null || (m_PreloadQueue.Count == 0 && m_CurrentBlock == null));

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

		BlockReader m_Reader;
		State m_State = State.StartNext;
		Queue<BlockPreloadEntry> m_PreloadQueue = new Queue<BlockPreloadEntry>();
		EnvData m_CurrentEnvData = new EnvData();
		EnvData m_PreUpdateEnvData = new EnvData();
		IStopCommand m_StopCommand;
		BlockEntry m_CurrentBlock;

		public BlockProcessor(BlockReader reader, ServiceContainer container, ResourceCache cache)
		{
			m_Reader = reader;
			m_Reader.Evaluator.SetEnvData(m_PreUpdateEnvData);
			Container = container;
			EnvDataHook = new EnvDataHook(container);
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

		void ClearPreload()
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

		public async Task Jump(BlockLabelInfo label, CancellationToken token)
		{
			var result = await m_Reader.Load(label.FileName, token);
			m_Reader.Seek(label);
			m_StopCommand = null;
			ClearPreload();
			EnvDataHook.PostJump(result.Meta, m_PreUpdateEnvData);
			m_State = State.StartNext;
		}

		public async Task<bool> Seek(Func<Block, bool> match, Func<Block, IEnvDataHolder, Task> onLoad, CancellationToken token)
		{
			m_Reader.Seek(CurrentLabel);
			bool first = true;
			Block prevBlock = null;
			while (m_Reader.TryRead(out var block))
			{
				if (!first && match(block))
				{
					if (onLoad != null)
					{
						await onLoad(prevBlock, Current);
					}
					PostSeek(block);
					return true;
				}
				else
				{
					if (!first)
					{
						m_Reader.BranchController.Save(m_CurrentEnvData);
					}
					first = false;
					UpdateCurrent(block);
					prevBlock = block;
				}
			}
			return false;

		}


		void UpdateCurrent(Block block)
		{
			EnvDataHook.PreUpdate(m_CurrentEnvData, block);
			foreach (var cmd in block.Commands)
			{
				cmd.SetContainer(Container);
				cmd.SetMetaData(block.Meta);
				cmd.UpdateEnvData(m_CurrentEnvData);
			}
			EnvDataHook.PostUpdate(m_CurrentEnvData, block);
			History.Add(m_CurrentEnvData, block, m_CurrentEnvData.Diff());
		}

		void PostSeek(Block block)
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

		public Task Back(int num, Func<Block, IEnvDataHolder, Task> onLoad, CancellationToken token)
		{
			var data = BackImpl(num);
			if (data != null)
			{
				return Restore(data, onLoad, token);
			}
			return Task.FromResult(true);
		}

		StoreData BackImpl(int num)
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
			var data = new StoreData
			{
				Label = History.CurrentLabel,
				Snapshot = snapshot,
				Logs = History.Save(),
			};
			return data;
		}


		void AddPreloadBlock(Block block)
		{
			EnvDataHook.PreUpdate(m_PreUpdateEnvData, block);
			foreach (var cmd in block.Commands)
			{
				cmd.SetContainer(Container);
				cmd.SetMetaData(block.Meta);
				cmd.UpdateEnvData(m_PreUpdateEnvData);
			}
			EnvDataHook.PostUpdate(m_PreUpdateEnvData, block);

			var diff = !block.SkipHistory ? m_PreUpdateEnvData.Diff() : new EnvDataDiff();
			var scope = new PreLoadScope(Cache);
			foreach (var cmd in block.Commands)
			{
				cmd.Initialize(scope);
			}
			m_PreloadQueue.Enqueue(new BlockPreloadEntry(block, scope, diff));
			m_Reader.BranchController.Save(m_PreUpdateEnvData);
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


		void ProcessPreload()
		{
			while (m_Reader.CanRead && CanPreload)
			{
				if (m_Reader.TryRead(out var block))
				{
					AddPreloadBlock(block);
				}
				else
				{
					return;
				}
			}
		}

		public void Process()
		{
			ProcessPreload();

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

		public async Task Restore(StoreData data, Func<Block, IEnvDataHolder, Task> onLoad, CancellationToken token)
		{
			await Jump(data.Label, token);

			m_CurrentEnvData.Load(data.Snapshot);
			m_PreUpdateEnvData.Load(data.Snapshot);
			m_Reader.BranchController.Load(m_CurrentEnvData);
			History.Load(data.Logs);

			if (!m_Reader.TryRead(out var block))
			{
				throw new Exception("");
			}

			m_StopCommand = block.StopCommand;
			m_State = State.Finished;

			if (onLoad != null)
			{
				await onLoad(block, Current);
			}
			ProcessPreload();
		}

	}

}