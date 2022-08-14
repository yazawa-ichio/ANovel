
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public abstract class CacheEntry : IDisposable
	{
		public int RefCount { get; private set; }

		public abstract bool IsLoaded { get; }
		public abstract Type CacheType { get; }

		public void RemoveRef()
		{
			RefCount--;
		}

		public void AddRef()
		{
			RefCount++;
		}

		public abstract void Dispose();

	}

	public class CacheEntry<T> : CacheEntry where T : class
	{
		string m_Path;
		IResourceLoader m_Loader;
		CancellationTokenSource m_CancellationTokenSource;
		Task<T> m_Task;
		bool m_Loaded;
		T m_Value;
		bool m_Disposed;

		public string Path => m_Path;

		public override bool IsLoaded => m_Loaded;

		public override Type CacheType => typeof(T);

		public T Value
		{
			get
			{
				if (IsLoaded)
				{
					return m_Value;
				}
				throw new CacheException(m_Path, "dont cache");
			}
		}

		public Exception Error { get; private set; }

		public bool IsDone
		{
			get
			{
				if (m_Disposed || IsLoaded)
				{
					return true;
				}
				return Error != null;
			}
		}

		public CacheEntry(string path, IResourceLoader loader, CancellationTokenSource cancellationTokenSource, Task<T> task)
		{
			m_Path = path;
			m_Loader = loader;
			m_CancellationTokenSource = cancellationTokenSource;
			m_Task = task;
			_ = Set(task);
		}

		async Task Set(Task<T> task)
		{
			try
			{
				m_Value = await task;
				m_Loaded = true;
				if (m_Disposed && m_Value != null)
				{
					m_Loader.Unload(Path, m_Value);
					m_Value = null;
				}
			}
			catch (Exception ex)
			{
				Error = ex;
			}
		}

		public Task<T> GetAsync()
		{
			if (m_Task == null) throw new ObjectDisposedException($"CacheEntry<{typeof(T)}>");
			return m_Task;
		}

		public override void Dispose()
		{
			m_Disposed = true;
			m_Task = null;
			m_CancellationTokenSource?.Cancel();
			m_CancellationTokenSource = null;
			if (m_Value != null)
			{
				m_Loader.Unload(Path, m_Value);
				m_Value = null;
			}
		}

	}


}