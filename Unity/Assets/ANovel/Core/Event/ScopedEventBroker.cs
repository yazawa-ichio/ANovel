using ANovel.Core;
using System;
using System.Collections.Generic;

namespace ANovel
{
	public class ScopedEventBroker : IDisposable
	{
		EventBroker m_Owner;
		List<object> m_List;

		public ScopedEventBroker(EventBroker owner)
		{
			m_Owner = owner;
			m_List = ListPool<object>.Pop();
		}

		public void Publish<T>(T name)
		{
			m_Owner.Publish(name);
		}

		public void Publish<T>(T name, object prm)
		{
			m_Owner.Publish(name, prm);
		}

		public void Subscribe(object obj)
		{
			m_List.Add(obj);
			m_Owner.Register(obj);
		}

		public void Unsubscribe(object obj)
		{
			m_List.Remove(obj);
		}

		public void Dispose()
		{
			if (m_Owner != null)
			{
				foreach (var obj in m_List)
				{
					m_Owner.Unregister(obj);
				}
				ListPool<object>.Push(m_List);
				m_List = null;
				m_Owner = null;
			}
		}

	}


}