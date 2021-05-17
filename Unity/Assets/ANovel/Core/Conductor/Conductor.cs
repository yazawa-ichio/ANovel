using ANovel.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public class Conductor : IDisposable
	{
		public int PreLoadCount { get => m_BlockProcessor.PreloadCount; set => m_BlockProcessor.PreloadCount = value; }

		public EventBroker Event { get; private set; } = new EventBroker();

		public ResourceCache Cache { get; private set; }

		public ServiceContainer Container { get; private set; } = new ServiceContainer();

		public ITextProcessor Text { get => m_BlockProcessor.Text; set => m_BlockProcessor.Text = value; }

		public EnvDataHook EnvDataHook => m_BlockProcessor.EnvDataHook;

		public event Action<Exception> OnError;

		public Func<Block, IEnvDataHolder, Task> OnLoad { get; set; }

		public bool IsStop => !m_Reader.CanRead && m_BlockProcessor.IsStop;

		public bool IsWaitNext => !IsLock && m_BlockProcessor.IsWaitNext;

		public bool IsLock => m_ErrorFlag || m_Locker.IsLock;

		public bool HasError => m_ErrorFlag;

		BlockReader m_Reader;
		bool m_ErrorFlag;
		CancellationTokenSource m_Cancellation = new CancellationTokenSource();
		BlockProcessor m_BlockProcessor;
		ScopeLocker m_Locker = new ScopeLocker();

		public Conductor(BlockReader reader, IResourceLoader loader)
		{
			m_Reader = reader;
			Cache = new ResourceCache(loader);
			Event.Register(this);
			Container.Set(Cache);
			Container.Set(Event);
			m_BlockProcessor = new BlockProcessor(Container, Cache);
		}

		public void Dispose()
		{
			m_BlockProcessor?.Dispose();
			m_BlockProcessor = null;
			Cache?.Dispose();
			Cache = null;
			Event?.Dispose();
			Event = null;
			Container?.Dispose();
			Container = null;
			m_Cancellation?.Cancel();
			m_Cancellation = null;
		}

		public void Update()
		{
			Process();
			Cache.ReleaseUnused();
		}

		public async Task Run(string path, string label, CancellationToken token)
		{
			using (m_Locker.ExclusiveLock())
			{
				m_ErrorFlag = false;
				m_BlockProcessor.Reset();
				if (OnLoad != null)
				{
					await OnLoad(null, m_BlockProcessor.Current);
				}
				await JumpImpl(new BlockLabelInfo(path, label, -1, -1), token);
			}
		}

		[EventSubscribe(ScenarioEvent.Jump)]
		async void Jump(ScenarioJumpEvent arg)
		{
			try
			{
				if (m_ErrorFlag)
				{
					return;
				}
				await Task.Yield();
				if (m_Cancellation != null)
				{
					await Jump(arg.Path, arg.Label, m_Cancellation.Token);
				}
			}
			catch (Exception err)
			{
				m_ErrorFlag = true;
				OnError?.Invoke(err);
			}
		}

		public Task Jump(string path, string label, CancellationToken token)
		{
			return Jump(new BlockLabelInfo(path, label, -1, -1), token);
		}

		public Task Jump(BlockLabelInfo label, CancellationToken token)
		{
			if (m_ErrorFlag) throw new InvalidOperationException("Conductor has error");
			if (IsLock) throw new InvalidOperationException($"already Loading");
			return JumpImpl(label, token);
		}

		async Task JumpImpl(BlockLabelInfo label, CancellationToken token)
		{
			using (m_Locker.Lock())
			{
				try
				{
					var result = await m_Reader.Load(label.FileName, token);
					m_Reader.Seek(label);
					m_BlockProcessor.PostJump(result);
				}
				catch (Exception)
				{
					m_ErrorFlag = true;
					throw;
				}
			}
		}

		public Task Seek(string label, CancellationToken token)
		{
			return Seek(block =>
			{
				return (block.LabelInfo.BlockIndex == 0 && block.LabelInfo.Name == label);
			}, token);
		}

		public async Task Seek(Func<Block, bool> match, CancellationToken token)
		{
			using (m_Locker.ExclusiveLock())
			{
				m_Reader.Seek(m_BlockProcessor.CurrentLabel);
				bool first = true;
				Block prevBlock = null;
				while (m_Reader.TryRead(out var block))
				{
					if (!first && match(block))
					{
						if (OnLoad != null)
						{
							await OnLoad(prevBlock, m_BlockProcessor.Current);
						}
						m_BlockProcessor.PostSeek(block);
						return;
					}
					else
					{
						first = false;
						m_BlockProcessor.UpdateCurrent(block);
						prevBlock = block;
					}
				}
				m_ErrorFlag = true;
				throw new Exception($"not found seek target");
			}
		}

		public Task Back(int num, CancellationToken token)
		{
			m_Locker.CheckLock();
			var data = m_BlockProcessor.Back(num);
			if (data != null)
			{
				return Restore(data, token);
			}
			return Task.FromResult(true);
		}

		void Process()
		{
			if (IsLock)
			{
				return;
			}
			try
			{
				ProcessPreload();
				m_BlockProcessor.Process();
			}
			catch (Exception ex)
			{
				m_ErrorFlag = true;
				if (OnError != null)
				{
					OnError(ex);
				}
				else
				{
					throw;
				}
			}
		}

		void ProcessPreload()
		{
			while (m_Reader.CanRead && m_BlockProcessor.CanPreload)
			{
				if (m_Reader.TryRead(out var block))
				{
					m_BlockProcessor.AddPreloadBlock(block);
				}
				else
				{
					return;
				}
			}
		}

		public bool TryNext()
		{
			if (IsLock)
			{
				return false;
			}
			if (m_BlockProcessor.TryNext())
			{
				Process();
				return true;
			}
			return false;
		}

		public StoreData Store()
		{
			return m_BlockProcessor.Store();
		}

		public async Task Restore(StoreData data, CancellationToken token)
		{
			if (data == null)
			{
				throw new Exception("not found data");
			}
			using (m_Locker.ExclusiveLock())
			{
				try
				{
					m_ErrorFlag = false;
					await JumpImpl(data.Label, token);
					if (!m_Reader.TryRead(out var block))
					{
						throw new Exception("");
					}
					m_BlockProcessor.Restore(data, block);
					if (OnLoad != null)
					{
						await OnLoad(block, m_BlockProcessor.Current);
					}
					ProcessPreload();
				}
				catch (Exception)
				{
					m_ErrorFlag = true;
					throw;
				}
			}
		}

	}
}