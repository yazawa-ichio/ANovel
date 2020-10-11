using System;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public interface ICacheHandle : IDisposable
	{
		bool IsLoaded { get; }
		bool Disposed { get; }
		Task GetAsync();
	}

	public class CacheHandle<T> : ICacheHandle
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

		Task ICacheHandle.GetAsync() => GetAsync();

		void CehckDisposed()
		{
			if (m_Entry == null)
			{
				throw new ObjectDisposedException($"already Disposed CacheHandle<{typeof(T)}>");
			}
		}

	}


}