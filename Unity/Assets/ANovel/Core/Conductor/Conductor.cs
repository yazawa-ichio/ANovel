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

		public IResourceCache Cache => m_Cache;

		public ServiceContainer Container { get; private set; } = new ServiceContainer();

		public ITextProcessor Text { get => m_BlockProcessor.Text; set => m_BlockProcessor.Text = value; }

		public EnvDataHook EnvDataHook => m_BlockProcessor.EnvDataHook;

		public IHistory History => m_BlockProcessor.History;

		public IVariableContainer Variables => Container.Get<IEvaluator>().Variables;

		public IVariableContainer GlobalVariables => Container.Get<IEvaluator>().GlobalVariables;

		public event Action<Exception> OnError;

		public Func<Block, IEnvDataHolder, Task> OnLoad { get; set; }

		public bool IsStop => m_BlockProcessor.IsStop;

		public bool IsWaitNext => !IsLock && m_BlockProcessor.IsWaitNext;

		public bool IsLock => m_ErrorFlag || m_Locker.IsLock;

		public bool HasError => m_ErrorFlag;

		bool m_ErrorFlag;
		CancellationTokenSource m_Cancellation = new CancellationTokenSource();
		BlockProcessor m_BlockProcessor;
		ScopeLocker m_Locker = new ScopeLocker();
		ResourceCache m_Cache;

		public Conductor(BlockReader reader, IResourceLoader loader)
		{
			m_Cache = new ResourceCache(loader);
			Event.Register(this);
			Container.Set<IEvaluator>(reader.Evaluator);
			Container.Set<IResourceCache>(m_Cache);
			Container.Set(Event);
			m_BlockProcessor = new BlockProcessor(reader, Container, m_Cache, reader.Evaluator);
		}

		public void Dispose()
		{
			m_BlockProcessor?.Dispose();
			m_BlockProcessor = null;
			m_Cache?.Dispose();
			m_Cache = null;
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
			m_Cache.ReleaseUnused();
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
					await m_BlockProcessor.Jump(label, token);
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
				if (await m_BlockProcessor.Seek(match, OnLoad, token))
				{
					return;
				}
				m_ErrorFlag = true;
				throw new Exception($"not found seek target");
			}
		}

		public Task Back(int num, CancellationToken token)
		{
			m_Locker.CheckLock();
			return m_BlockProcessor.Back(num, OnLoad, token);
		}

		public Task Back(IHistoryLog log, CancellationToken token)
		{
			var num = m_BlockProcessor.History.GetBackNum(log);
			return Back(num, token);
		}

		void Process()
		{
			if (IsLock)
			{
				return;
			}
			try
			{
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
					await m_BlockProcessor.Restore(data, OnLoad, token);
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