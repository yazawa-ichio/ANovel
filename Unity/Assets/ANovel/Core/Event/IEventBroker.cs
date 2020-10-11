using ANovel.Core;
using System;

namespace ANovel
{
	public interface IEventBroker
	{
		EventPublisher Publisher();
		EventPublisher<T> Publisher<T>();
	}

	public static class EventBrokerExtension
	{
		public static void Publish(this IEventBroker self, string name)
		{
			self.Publisher().Publish(name);
		}

		public static void Publish<TEvent>(this IEventBroker self, TEvent name) where TEvent : Enum
		{
			self.Publisher().Publish(EventNameToStrConverter.ToStr(name));
		}

		public static void Publish<TValue>(this IEventBroker self, string name, TValue args)
		{
			self.Publisher<TValue>().Publish(name, args);
		}

		public static void Publish<TEvent, TValue>(this IEventBroker self, TEvent name, TValue args) where TEvent : Enum
		{
			self.Publisher<TValue>().Publish(EventNameToStrConverter.ToStr(name), args);
		}

	}

}