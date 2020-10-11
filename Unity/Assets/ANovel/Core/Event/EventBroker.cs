using System;
using System.Collections.Generic;

namespace ANovel.Core
{

	public class EventBroker : IEventBroker, IDisposable
	{
		EventPublisher m_Publisher = new EventPublisher();
		Dictionary<Type, IEventPublisher> m_ValuePublishers = new Dictionary<Type, IEventPublisher>();

		void CheckDiposed()
		{
			if (m_Publisher == null)
			{
				throw new ObjectDisposedException("EventBroker");
			}
		}

		public EventPublisher Publisher()
		{
			CheckDiposed();
			return m_Publisher;
		}

		public EventPublisher<T> Publisher<T>()
		{
			CheckDiposed();
			if (!m_ValuePublishers.TryGetValue(typeof(T), out var holder))
			{
				m_ValuePublishers[typeof(T)] = holder = new EventPublisher<T>();
			}
			return holder as EventPublisher<T>;
		}

		public ScopedEventBroker Scoped(object owner)
		{
			CheckDiposed();
			return new ScopedEventBroker(this, owner);
		}

		public EventSubscribeAccessor Subscribe(object owner, string name)
		{
			CheckDiposed();
			return new EventSubscribeAccessor(this, owner, name);
		}

		public EventSubscribeAccessor Subscribe<TEvent>(object owner, TEvent name) where TEvent : Enum
		{
			CheckDiposed();
			return new EventSubscribeAccessor(this, owner, EventNameToStrConverter.ToStr(name));
		}

		public void Unsubscribe(object owner)
		{
			CheckDiposed();
			m_Publisher.Unsubscribe(owner);
			foreach (var value in m_ValuePublishers.Values)
			{
				value.Unsubscribe(owner);
			}
		}

		public void Dispose()
		{
			m_Publisher = null;
			m_ValuePublishers = null;
		}

	}
}