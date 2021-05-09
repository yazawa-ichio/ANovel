using NUnit.Framework;
using System;
using UnityEngine.TestTools.Constraints;
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
			broker.Register(handler);
			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(1, handler.Total, "イベントが発行される");
			broker.Publish(Handler.Event.Add, 4);
			Assert.AreEqual(5, handler.Total, "引数付きイベントが発行される");

			Assert.Throws<ArgumentException>(() =>
			{
				broker.Publish(Handler.Event.Add, 4f);
			}, "型が違うのでエラー");

			broker.Unregister(handler);
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
				scope.Publish(Handler.Event.Increment);
				Assert.AreEqual(1, handler.Total, "イベントが発行される")
					;
				scope.Publish(Handler.Event.Add, 4);
				Assert.AreEqual(5, handler.Total, "引数付きイベントが発行される");

				Assert.Throws<ArgumentException>(() =>
				{
					scope.Publish(Handler.Event.Add, 4f);
				}, "型が違うのでエラー");
			}
			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(5, handler.Total, "スコープの機能で購読を解除");
		}

		[Test]
		public void 購読解除()
		{
			EventBroker broker = new EventBroker();
			Handler handler = new Handler();
			broker.Register(handler);

			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(1, handler.Total, "イベントが発行される")
				;
			broker.Publish(Handler.Event.Add, 4);
			Assert.AreEqual(5, handler.Total, "引数付きイベントが発行される");

			broker.Unregister(handler);
			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(5, handler.Total, "購読解除したので発火されない");

			broker.Dispose();

			Assert.Throws<ObjectDisposedException>(() =>
			{
				broker.Register(handler);
			}, "解放の登録はエラー");

			Assert.Throws<ObjectDisposedException>(() =>
			{
				broker.Publish(Handler.Event.Increment);
			}, "イベントの発火もエラー");
		}

		[Test]
		public void 多重登録は無効になる()
		{
			EventBroker broker = new EventBroker();
			Handler handler = new Handler();
			broker.Register(handler);
			broker.Register(handler);

			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(1, handler.Total, "一度しか実行されない");

			broker.Unregister(handler);

			broker.Publish(Handler.Event.Increment);
			Assert.AreEqual(1, handler.Total, "一度で解除される");

		}

		[Test]
		public void イベント中のイベント登録()
		{
			EventBroker broker = new EventBroker();
			Handler handler1 = new Handler();
			Handler handler2 = new Handler();
			Handler handler3 = new Handler();

			Action register = default;
			int registerCount = 0;

			register = () =>
			{
				registerCount++;
				broker.Register(handler2);
				broker.Register(handler3);
			};

			broker.Register(handler1);

			broker.Publish(Handler.Event.Action, register);

			Assert.AreEqual(1, registerCount, "イベント登録前に実行したイベントは発火されない");

			broker.Publish(Handler.Event.Action, register);

			Assert.AreEqual(1 + 3, registerCount, "両方登録されている");

			register = () =>
			{
				registerCount++;
				broker.Unregister(handler2);
			};

			broker.Publish(Handler.Event.Action, register);

			Assert.AreEqual(1 + 3 + 2, registerCount, "イベントは発火中のUnsubscribeは即時実行される");

		}

		[Test]
		public void イベント名変換()
		{
			var val = Handler.Event.Increment;
			object box = Handler.Event.Increment;
			Assert.AreEqual(EventNameToStrConverter.ToStr(val), EventNameToStrConverter.ToStr(box), "ボックス化しても結果は同じ");
			Assert.AreEqual("test", EventNameToStrConverter.ToStr("test"), "文字列はそのまま");


			EventNameToStrConverter.ToStr(Handler.Event.Add);
			Assert.That(() =>
			{
				EventNameToStrConverter.ToStr(Handler.Event.Add);
			}, Is.Not.AllocatingGCMemory(), "キャッシュ済みの場合はEventのGCが発生しない");

		}

		class Handler
		{
			public enum Event
			{
				Increment,
				Add,
				Action,
			}

			public int Total;

			[EventSubscribe(Event.Increment)]
			public void Increment() => Total++;

			[EventSubscribe(Event.Add)]
			public void Add(int val) => Total += val;

			[EventSubscribe(Event.Action)]
			public void Action(Action val) => val?.Invoke();

		}

	}
}