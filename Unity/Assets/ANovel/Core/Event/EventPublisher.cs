using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public class EventPublisher : EventPublisherBase<EventPublisher.Entry, object>
	{
		public readonly struct Entry : IEvent<object>
		{
			readonly Action m_Action;

			public object Owner { get; }

			public Entry(object owner, Action action)
			{
				Owner = owner;
				m_Action = action;
			}

			public bool IsSameAction(object action)
			{
				return m_Action.Equals(action);
			}

			public void Invoke(object value)
			{
				m_Action();
			}
		}

		public void Publish(string name)
		{
			PublishImpl(name, default);
		}

		public void Publish<T>(T name) where T : Enum
		{
			PublishImpl(EventNameToStrConverter.ToStr(name), default);
		}

		public void Unsubscribe(string name, Action action)
		{
			UnsubscribeImpl(name, action);
		}

		public void Unsubscribe<T>(T name, Action action)
		{
			UnsubscribeImpl(EventNameToStrConverter.ToStr(name), action);
		}

		public void Subscribe(object owner, string name, Action action)
		{
			if (!m_Dic.TryGetValue(name, out var entries))
			{
				m_Dic[name] = entries = new List<Entry>();
			}
			entries.Add(new Entry(owner, action));
		}

		public void Subscribe<T>(object owner, T name, Action action)
		{
			Subscribe(owner, EventNameToStrConverter.ToStr(name), action);
		}

	}


	public class EventPublisher<TValue> : EventPublisherBase<EventPublisher<TValue>.Entry, TValue>
	{
		public readonly struct Entry : IEvent<TValue>
		{
			readonly Action<TValue> m_Action;

			public object Owner { get; }

			public Entry(object owner, Action<TValue> action)
			{
				Owner = owner;
				m_Action = action;
			}

			public bool IsSameAction(object action)
			{
				return m_Action.Equals(action);
			}

			public void Invoke(TValue value)
			{
				m_Action(value);
			}

		}

		public void Publish(string name, TValue value)
		{
			PublishImpl(name, value);
		}

		public void Publish<TEvent>(TEvent name, TValue value) where TEvent : Enum
		{
			PublishImpl(EventNameToStrConverter.ToStr(name), value);
		}

		public void Unsubscribe(string name, Action<TValue> action)
		{
			UnsubscribeImpl(name, action);
		}

		public void Unsubscribe<TEvent>(TEvent name, Action<TValue> action) where TEvent : Enum
		{
			UnsubscribeImpl(EventNameToStrConverter.ToStr(name), action);
		}

		public void Subscribe(object owner, string name, Action<TValue> action)
		{
			if (!m_Dic.TryGetValue(name, out var entries))
			{
				m_Dic[name] = entries = new List<Entry>();
			}
			entries.Add(new Entry(owner, action));
		}

		public void Subscribe<TEvent>(object owner, TEvent name, Action<TValue> action) where TEvent : Enum
		{
			Subscribe(owner, EventNameToStrConverter.ToStr(name), action);
		}

	}


}