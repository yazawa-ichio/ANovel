using NUnit.Framework;
using System;
using UnityEngine.TestTools.Constraints;
//using Assert = UnityEngine.Assertions.Assert;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace ANovel.Core.Tests
{
	public class EventTests
	{
		[Test]
		public void イベントテスト()
		{
			EventBroker broker = new EventBroker();
			Handler handler = new Handler();
			handler.Register(broker);
			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(1, handler.Total, "イベントが発行される");
			broker.Publish(Handler.Event.Sum, 4);
			Assert.AreEqual(5, handler.Total, "引数付きイベントが発行される");
			broker.Publish(Handler.Event.Sum, 4f);
			Assert.AreEqual(5, handler.Total, "型が違うと無視される");

			broker.Unsubscribe(handler);
			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(5, handler.Total, "オブジェクトを指定して購読を破棄したのでイベントが発行されない");

		}

		[Test]
		public void イベントスコープ()
		{
			EventBroker broker = new EventBroker();
			Handler handler = new Handler();
			using (var scope = broker.Scoped(handler))
			{
				handler.Register(scope);

				broker.Publisher().Publish(Handler.Event.Increment);
				Assert.AreEqual(1, handler.Total, "イベントが発行される")
					;
				broker.Publisher<int>().Publish(Handler.Event.Sum, 4);
				Assert.AreEqual(5, handler.Total, "引数付きイベントが発行される");

				broker.Publisher<float>().Publish(Handler.Event.Sum, 4f);
				Assert.AreEqual(5, handler.Total, "型が違うと無視される");
			}
			broker.Publish(Handler.Event.Increment);
			broker.Publisher().Publish(Handler.Event.Increment);
			Assert.AreEqual(5, handler.Total, "スコープの機能で購読を解除");
		}

		[Test]
		public void 購読解除()
		{
			EventBroker broker = new EventBroker();
			Handler handler = new Handler();
			handler.RegisterBroker(broker);

			broker.Publisher().Publish(Handler.Event.Increment);
			Assert.AreEqual(1, handler.Total, "イベントが発行される")
				;
			broker.Publisher<int>().Publish(Handler.Event.Sum, 4);
			Assert.AreEqual(5, handler.Total, "引数付きイベントが発行される");

			handler.UnregisterIncrement(broker);
			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(5, handler.Total, "購読解除したので発火されない");

			broker.Publisher<int>().Publish(Handler.Event.Sum, 4);
			Assert.AreEqual(9, handler.Total, "一つだけ解除出来ている");

			handler.UnregisterSum(broker);
			broker.Publisher().Publish(Handler.Event.Increment);
			Assert.AreEqual(9, handler.Total, "Sumの購読を解除");

			broker.Dispose();

			Assert.Throws(typeof(ObjectDisposedException), () =>
			{
				broker.Publish(Handler.Event.Increment);
			}, "");
		}


		[Test]
		public void イベント中のイベント登録()
		{
			EventBroker broker = new EventBroker();
			Handler handler = new Handler();

			Action register = default;
			int registerCount = 0;

			register = () =>
			{
				registerCount++;
				handler.RegisterBroker(broker);
				Assert.AreEqual(0, handler.Total, "イベントに登録してもすぐには発火されない");
				broker.Unsubscribe(broker);
				handler.Unregister(broker);
				broker.Publish(Handler.Event.Increment);
				Assert.AreEqual(0, handler.Total, "イベントの実行中の箇所で登録解除しても正常に解除される");

				handler.RegisterBroker(broker);
				handler.UnregisterSum(broker);
				broker.Publish(Handler.Event.Increment);

			};

			broker.Publisher().Subscribe(broker, Handler.Event.Increment, register);

			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(1, handler.Total, "register内でのIncrementを受け取れている");

			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(2, handler.Total, "イベント中にIncrementは再登録されている");

			broker.Publish(Handler.Event.Sum, 10);
			Assert.AreEqual(2, handler.Total, "イベント中にSumは解除済み");

			Assert.AreEqual(1, registerCount, "registerイベントは一回だけ発火されている");

		}

		[Test]
		public void イベント名テスト()
		{
			EventBroker broker = new EventBroker();
			string name = EventNameToStrConverter.ToStr(Handler.Event.Increment);

			int count = 0;
			broker.Subscribe(broker, name).Register(() => count++);

			broker.Publish(Handler.Event.Increment);

			Assert.AreEqual(1, count, "Enumは文字列になるので実行される");
		}


		[Test]
		public void イベント名変換()
		{
			var val = Handler.Event.Increment;
			object box = Handler.Event.Increment;
			Assert.AreEqual(EventNameToStrConverter.ToStr(val), EventNameToStrConverter.ToStr(box), "ボックス化しても結果は同じ");
			Assert.AreEqual("test", EventNameToStrConverter.ToStr("test"), "文字列はそのまま");


			EventNameToStrConverter.ToStr(Handler.Event.Sum);
			Assert.That(() =>
			{
				EventNameToStrConverter.ToStr(Handler.Event.Sum);
			}, Is.Not.AllocatingGCMemory(), "キャッシュ済みの場合はEventのGCが発生しない");

		}

		class Handler
		{
			public enum Event
			{
				Increment,
				Sum,
			}

			public int Total;

			public void Increment() => Total++;

			public void Sum(int val) => Total += val;

			public void Register(IEventBroker broker)
			{
				broker.Publisher().Subscribe(this, Event.Increment, Increment);
				broker.Publisher<int>().Subscribe(this, Event.Sum, Sum);
			}

			public void Register(ScopedEventBroker broker)
			{
				broker.Subscribe(Event.Increment).Register(Increment);
				broker.Subscribe(Event.Sum).Register<int>(Sum);
			}

			public void RegisterBroker(EventBroker broker)
			{
				broker.Subscribe(this, Event.Increment).Register(Increment);
				broker.Subscribe(this, Event.Sum).Register<int>(Sum);
			}

			public void UnregisterIncrement(EventBroker broker)
			{
				broker.Subscribe(this, Event.Increment).Unregister(Increment);
			}

			public void UnregisterSum(EventBroker broker)
			{
				broker.Subscribe(this, Event.Sum).Unregister<int>(Sum);
			}

			public void Unregister(EventBroker broker)
			{
				broker.Publisher().Unsubscribe(Event.Increment, Increment);
				broker.Publisher<int>().Unsubscribe(Event.Sum, Sum);
			}

		}

	}
}