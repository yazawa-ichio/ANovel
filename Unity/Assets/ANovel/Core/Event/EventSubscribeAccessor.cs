using System;

namespace ANovel.Core
{
	public readonly struct EventSubscribeAccessor
	{
		readonly IEventBroker m_Broker;
		readonly object m_Owner;
		readonly string m_Name;

		public EventSubscribeAccessor(IEventBroker broker, object owner, string name)
		{
			m_Broker = broker;
			m_Owner = owner;
			m_Name = name;
		}

		public void Register(Action action)
		{
			m_Broker.Publisher().Subscribe(m_Owner, m_Name, action);
		}

		public void Register<T>(Action<T> action)
		{
			m_Broker.Publisher<T>().Subscribe(m_Owner, m_Name, action);
		}

		public void Unregister(Action action)
		{
			m_Broker.Publisher().Unsubscribe(m_Name, action);
		}

		public void Unregister<T>(Action<T> action)
		{
			m_Broker.Publisher<T>().Unsubscribe(m_Name, action);
		}

	}

}