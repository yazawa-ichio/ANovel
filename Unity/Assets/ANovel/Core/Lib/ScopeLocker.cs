using System;
namespace ANovel.Core
{

	public class ScopeLocker
	{
		public bool IsLock => m_Count > 0;
		int m_Count;
		object m_Lock = new object();

		public void CheckLock()
		{
			if (m_Count > 0)
			{
				throw new Exception("this Scope Locked");
			}
		}

		public IDisposable ExclusiveLock()
		{
			lock (m_Lock)
			{
				if (m_Count > 0)
				{
					throw new Exception("this Scope Locked");
				}
				m_Count++;
				return new Handle(this);
			}
		}

		public IDisposable Lock()
		{
			lock (m_Lock)
			{
				m_Count++;
				return new Handle(this);
			}
		}

		void Unlock()
		{
			lock (m_Lock)
			{
				m_Count--;
			}
		}

		class Handle : IDisposable
		{
			ScopeLocker m_Locker;

			public Handle(ScopeLocker locker)
			{
				m_Locker = locker;
			}

			public void Dispose()
			{
				m_Locker.Unlock();
				m_Locker = null;
			}

		}

	}

}