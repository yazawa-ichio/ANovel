using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ANovel.Core.Tests
{
	public class ConductorTests
	{
		[UnityTest]
		public IEnumerator コンダクターテスト()
		{
			var reader = new BlockReader(new TestDataLoader());
			var loader = new Loader();
			var text = new Text();
			var conductor = new Conductor(reader, loader, text);

			conductor.Load("ConductorTest.anovel").Wait();

			conductor.Update();
			Assert.AreEqual("テキスト表示", text.TextBlock.Get());
			conductor.Update();
			Assert.AreEqual("テキスト表示", text.TextBlock.Get());

			Assert.AreEqual(2, loader.LoadCount);
			Assert.IsTrue(conductor.TryNext(), "シナリオを進める");

			Assert.Null(text.TextBlock, "プリロード中なのでテキストが読み取れない");

			while (text.TextBlock == null)
			{
				conductor.Update();
				yield return null;
			}

			conductor.Update();
			Assert.AreEqual("【名前】", text.TextBlock.GetLine(0));
			Assert.AreEqual("名前付きテキスト表示", text.TextBlock.GetLine(1));

			text.Next = false;
			Assert.IsFalse(conductor.TryNext(), "テキスト表示中など次に進めない");
			text.Next = true;
			Assert.IsTrue(conductor.TryNext(), "シナリオを進める");

			Assert.Null(text.TextBlock);

			Assert.IsFalse(conductor.TryNext(), "同期コマンドが終わるまでテキストには進まない");
			conductor.Update();

			Assert.Null(text.TextBlock);
			Assert.IsFalse(conductor.TryNext(), "同期コマンドが終わるまでテキストには進まない");
			conductor.Update();

			TestSync.EndFlag = true;
			conductor.Update();
			Assert.AreEqual("同期が終わるまでテキストまで進まない", text.TextBlock.Get());

			Assert.IsTrue(conductor.TryNext(), "シナリオを進める");

			Assert.IsFalse(conductor.IsRunning, "終端まで進んだ");


			conductor.Dispose();

		}

		[Test]
		public void 読み込みエラーテスト()
		{
			var reader = new BlockReader(new TestDataLoader());
			var loader = new Loader();
			var text = new Text();
			var conductor = new Conductor(reader, loader, text);
			Exception error = null; ;
			try
			{
				conductor.Load("Error").Wait();
			}
			catch (Exception ex)
			{
				error = ex;
			}
			Assert.NotNull(error);
		}

		[Test]
		public void サービスコンテナ()
		{
			var container = new ServiceContainer();
			Assert.Throws<KeyNotFoundException>(() =>
			{
				container.Get<DummyData>();
			});
			Assert.IsFalse(container.TryGet<DummyData>(out _));
			var service = new DummyData();
			container.Set(typeof(DummyData), service);

			Assert.AreEqual(service, container.Get<DummyData>());
			Assert.IsTrue(container.TryGet<DummyData>(out var ret));
			Assert.AreEqual(service, ret);

			var child = container.CreateChild();

			Assert.AreEqual(service, child.Get<DummyData>(), "親のサービスを受け取れている");
			Assert.IsTrue(child.TryGet<DummyData>(out ret));
			Assert.AreEqual(service, ret, "親のサービスを受け取れている");

			child.Set(new DummyData());
			Assert.AreNotEqual(service, child.Get<DummyData>(), "子が優先される");
			Assert.IsTrue(child.TryGet<DummyData>(out ret));
			Assert.AreNotEqual(service, ret, "子が優先される");

		}

		class Loader : IResourceLoader
		{
			public int LoadCount;

			public Task<T> Load<T>(string path, CancellationToken token) where T : Object
			{
				LoadCount++;
				return Task.FromResult<T>(null);
			}
			public async Task<T> LoadRaw<T>(string path, CancellationToken token)
			{
				LoadCount++;
				await Task.Delay(100);
				return System.Activator.CreateInstance<T>();
			}
		}

		class Text : ITextProcessor
		{
			public ServiceContainer Container { get; private set; }

			public TextBlock TextBlock { get; private set; }

			public bool Next { get; set; } = true;

			public bool IsProcessing => true;

			public void Init(ServiceContainer container)
			{
				Container = container;
			}
			public void Set(TextBlock text)
			{
				TextBlock = text;
			}
			public bool TryNext()
			{
				if (Next)
				{
					TextBlock = null;
					return true;
				}
				return false;
			}
		}

		class DummyData { }

		[CommandName("test_preload")]
		class TestPreLoad : Command
		{
			[CommandField(Required = true)]
			string m_Path = default;

			protected override void Preload(IPreLoader loader)
			{
				loader.LoadRaw<DummyData>(m_Path);
			}
		}

		[CommandName("test_sync")]
		public class TestSync : Command
		{
			public static bool EndFlag = false;

			public override bool IsSync() => true;

			public override bool IsEnd() => EndFlag;

			protected override void Initialize()
			{
				Assert.IsTrue(TryGet<IPreLoader>(out _));
				Assert.NotNull(Get<EventBroker>());
			}

			public override void FinishBlock()
			{
				EndFlag = false;
			}

		}

		[CommandName("test_prepare_wait")]
		public class TestPrepareWaitTime : Command
		{
			[CommandField(Required = true)]
			float m_Time = default;

			float m_Start;

			protected override void Initialize()
			{
				m_Start = Time.time;
			}

			public override bool IsPrepared()
			{
				return m_Start + m_Time < Time.time;
			}

		}




	}
}