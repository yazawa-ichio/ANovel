using System;

namespace ANovel.Core
{
	public struct ScopedEventBroker : IEventBroker, IDisposable
	{
		EventBroker m_EventBroker;
		object m_Owner;

		public ScopedEventBroker(EventBroker broker, object owner)
		{
			m_Owner = owner;
			m_EventBroker = broker;
		}

		public EventPublisher Publisher()
		{
			return m_EventBroker.Publisher();
		}

		public EventPublisher<T> Publisher<T>()
		{
			return m_EventBroker.Publisher<T>();
		}

		public EventSubscribeAccessor Subscribe(string name)
		{
			return new EventSubscribeAccessor(this, m_Owner, name);
		}

		public EventSubscribeAccessor Subscribe<TEvent>(TEvent name) where TEvent : Enum
		{
			return new EventSubscribeAccessor(this, m_Owner, EventNameToStrConverter.ToStr(name));
		}

		public void Dispose()
		{
			m_EventBroker?.Unsubscribe(m_Owner);
			m_EventBroker = null;
			m_Owner = null;
		}

	}
}