using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ANovel.Core
{

	public class Conductor : IDisposable
	{
		protected bool m_EndBlock;
		protected BlockReader m_Reader;
		protected BlockEntry m_Current;
		protected Queue<BlockEntry> m_PreloadQueue = new Queue<BlockEntry>();
		protected bool m_WaitTextProcess;
		protected bool m_IsLoad;

		public int PreLoadCount { get; set; } = 3;

		public EventBroker Event { get; private set; } = new EventBroker();

		public ResourceCache Cache { get; private set; }

		public ServiceContainer Container { get; private set; } = new ServiceContainer();

		protected ITextProcessor Text { get; private set; }

		public bool IsRunning => !(m_PreloadQueue.Count == 0 && m_Current == null);

		public bool IsProcessing
		{
			get
			{
				if (m_Current == null)
				{
					return false;
				}
				if (Text != null && Text.IsProcessing)
				{
					return true;
				}
				return m_Current.IsProcessing;
			}
		}

		protected Conductor() { }

		public Conductor(BlockReader reader, IResourceLoader loader, ITextProcessor text)
		{
			m_Reader = reader;
			Cache = new ResourceCache(loader);
			Text = text;
			Container.Set(Cache);
			Container.Set(Event);
		}

		public virtual void Update()
		{
			Process();
			Cache.ReleaseUnused();
		}

		public async Task Load(string path)
		{
			if (m_IsLoad) throw new InvalidOperationException($"already Loading {path}");
			try
			{
				m_IsLoad = true;
				await m_Reader.Load(path);
				m_IsLoad = false;
			}
			catch (Exception)
			{
				m_IsLoad = false;
				throw;
			}
		}

		public virtual void Process()
		{
			ProcessPreload();
			ProcessCommand();
		}

		protected virtual void ProcessPreload()
		{
			if (m_EndBlock || m_Reader.EndOfFile)
			{
				return;
			}
			while (m_PreloadQueue.Count < PreLoadCount)
			{
				var block = new Block();
				if (m_Reader.TryRead(block))
				{
					var container = Container.CreateChild();
					var scope = new PreLoadScope(Cache);
					container.Set<IPreLoader>(scope);
					foreach (var cmd in block.Commands)
					{
						cmd.Initialize(container);
					}
					var entry = new BlockEntry(block, scope);
					m_PreloadQueue.Enqueue(entry);
					m_EndBlock = entry.IsEndBlock = m_Reader.EndOfFile;
				}
				else
				{
					return;
				}
			}
		}

		protected virtual void ProcessCommand()
		{
			if (m_Current == null)
			{
				if (m_PreloadQueue.Count == 0 || !m_PreloadQueue.Peek().IsPrepared)
				{
					return;
				}
				m_Current = m_PreloadQueue.Dequeue();
				ProcessPreload();
			}
			if (m_Current.TryProcess())
			{
				if (!m_WaitTextProcess)
				{
					Text?.Set(m_Current.Block.Text);
					m_WaitTextProcess = true;
				}
			}
		}

		public bool TryNext()
		{
			if (!m_WaitTextProcess)
			{
				m_Current?.TryNext();
				return false;
			}
			if (Text != null && !Text.TryNext())
			{
				return false;
			}
			m_WaitTextProcess = false;
			if (m_Current != null)
			{
				m_Current.Finish();
				m_Current = null;
			}
			Process();
			return true;
		}

		public virtual void Dispose()
		{
			Cache?.Dispose();
			Cache = null;
			Event?.Dispose();
			Event = null;
			Container?.Dispose();
			Container = null;
		}

	}
}