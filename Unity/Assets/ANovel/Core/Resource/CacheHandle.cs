using System;
using System.Threading.Tasks;

namespace ANovel.Core
{


	internal class CacheHandle<T> : ICacheHandle<T> where T : class
	{
		CacheEntry<T> m_Entry;

		public bool IsLoaded
		{
			get
			{
				CehckDisposed();
				return m_Entry.IsLoaded;
			}
		}

		public bool IsDone
		{
			get
			{
				return Disposed || m_Entry.IsDone;
			}
		}

		public T Value
		{
			get
			{
				CehckDisposed();
				return m_Entry.Value;
			}
		}

		public bool Disposed => m_Entry == null;

		public CacheHandle(CacheEntry<T> entry)
		{
			m_Entry = entry;
			m_Entry.AddRef();
		}

		~CacheHandle()
		{
			Dispose();
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			m_Entry?.RemoveRef();
			m_Entry = null;
		}

		public Task<T> GetAsync()
		{
			return m_Entry.GetAsync();
		}

		public ICacheHandle<T> Duplicate()
		{
			return new CacheHandle<T>(m_Entry);
		}

		Task ICacheHandle.GetAsync() => GetAsync();

		public void CheckError()
		{
			if (m_Entry != null && m_Entry.Error != null)
			{
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(m_Entry.Error).Throw();
			}
		}

		void CehckDisposed()
		{
			if (m_Entry == null)
			{
				throw new ObjectDisposedException($"already Disposed CacheHandle<{typeof(T)}>");
			}
		}

	}


}