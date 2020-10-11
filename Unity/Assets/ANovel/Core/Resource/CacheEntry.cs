
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

	public class CacheEntry<T> : CacheEntry
	{
		string m_Path;
		IResourceLoader m_Loader;
		CancellationTokenSource m_CancellationTokenSource;
		Task<T> m_Task;
		bool m_Loaded;
		T m_Value;

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
			m_Value = await task;
			m_Loaded = true;
		}

		public Task<T> GetAsync()
		{
			if (m_Task == null) throw new ObjectDisposedException($"CacheEntry<{typeof(T)}>");
			return m_Task;
		}

		public override void Dispose()
		{
			m_Task = null;
			m_CancellationTokenSource?.Cancel();
			m_CancellationTokenSource = null;
		}

	}


}