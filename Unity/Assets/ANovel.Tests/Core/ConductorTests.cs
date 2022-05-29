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
			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			var loader = new Loader();
			var text = new Text();
			var conductor = new Conductor(reader, loader)
			{
				Text = text
			};
			conductor.OnLoad = text.OnLoad;
			conductor.Run("ConductorTest", null, CancellationToken.None).Wait();
			conductor.Update();
			Assert.AreEqual("テキスト表示", text.TextBlock.Get());
			conductor.Update();
			Assert.AreEqual("テキスト表示", text.TextBlock.Get());

			Assert.AreEqual(2, loader.LoadCount);
			var prevBlock = text.TextBlock;
			Assert.IsTrue(conductor.TryNext(), "シナリオを進める");

			Assert.AreEqual(prevBlock, text.TextBlock, "プリロード中なのでテキストが読み取れない");

			while (text.TextBlock == prevBlock)
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

			Assert.IsTrue(conductor.IsStop, "終端まで進んだ");


			conductor.Dispose();

		}

		[Test]
		public void エラーテスト()
		{
			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			var loader = new Loader();
			var text = new Text();
			var conductor = new Conductor(reader, loader)
			{
				Text = text
			};
			conductor.OnLoad = text.OnLoad;
			Exception error = null; ;
			conductor.OnError += (e) => error = e;
			try
			{
				conductor.Run("Error", null, CancellationToken.None).Wait();
			}
			catch (Exception ex)
			{
				error = ex;
			}
			Assert.NotNull(error);
			Assert.IsTrue(conductor.HasError);
			error = null;

			conductor.Run("ConductorErrorTest", null, CancellationToken.None).Wait();
			Assert.IsNull(error);

			conductor.Update();

			Assert.IsTrue(conductor.HasError);
			Assert.IsNotNull(error, "Update時のエラーはOnErrorで受け取る");

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

		[Test]
		public void マニュアルジャンプテスト()
		{
			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			var loader = new Loader();
			var text = new Text();
			var conductor = new Conductor(reader, loader)
			{
				Text = text
			};
			conductor.OnLoad = text.OnLoad;
			StoreData store;
			conductor.Run("ConductorJumpTest", "", CancellationToken.None).Wait();
			{
				conductor.Update();
				Assert.AreEqual("ジャンプテスト開始", text.TextBlock.Get());
				conductor.TryNext();
				conductor.Update();
				Assert.AreEqual("停止", text.TextBlock.Get());
				Assert.IsFalse(conductor.IsStop, "終了ブロックではない");
				conductor.TryNext();
				conductor.Update();
				Assert.IsTrue(conductor.IsStop, "終了ブロックに来た");
				Assert.AreEqual("停止", text.TextBlock.Get(), "stopの終了ブロックなので文字は消えない");
			}
			// ロールバック機能
			{
				conductor.Back(1, CancellationToken.None).Wait();
				Assert.IsFalse(conductor.IsStop, "終了ブロック前に戻る");
				Assert.AreEqual("ジャンプテスト開始", text.TextBlock.Get(), "stopの終了ブロックはヒストリーに記録されないので二つ前に戻る");
				conductor.Back(1, CancellationToken.None).Wait();

				Assert.AreEqual("ジャンプテスト開始", text.TextBlock.Get(), "stopの終了ブロックはヒストリーに記録されないので二つ前に戻る");

				for (int i = 0; i < 100 && !conductor.IsStop; i++)
				{
					conductor.TryNext();
					conductor.Update();
				}
				Assert.IsTrue(conductor.IsStop, "終端にいる");
			}
			// Jump機能テスト
			{
				conductor.Jump("ConductorJumpTest", "jump1", CancellationToken.None).Wait();
				conductor.Update();
				Assert.AreEqual("ジャンプ先1", text.TextBlock.Get());
				Assert.IsFalse(conductor.IsStop, "終了ブロックではない");
				conductor.TryNext();
				conductor.Update();
				Assert.IsTrue(conductor.IsStop, "終了ブロックに来た");
				Assert.AreEqual("ジャンプ先1", text.TextBlock.Get(), "stopの終了ブロックなので文字は消えない");

				store = conductor.Store();
				Assert.NotNull(store, "セーブデータ");
				Assert.NotNull(Packer.PackAndToJson(store), "セーブデータをシリアライズ出来る");
			}
			{

				Assert.IsFalse(conductor.HasError);

				Assert.Throws<AggregateException>(() =>
				{
					conductor.Seek("jump1", CancellationToken.None).Wait();
				}, "シーク先が無いのでエラーになる");

				Assert.IsTrue(conductor.HasError, "エラーフラグが立つ");

				Assert.Throws<InvalidOperationException>(() =>
				{
					conductor.Jump("ConductorJumpTest", "jump1", CancellationToken.None).Wait();
				}, "エラー後にJump操作は出来ない");

				conductor.Restore(Packer.Unpack<StoreData>(Packer.Pack(store)), CancellationToken.None).Wait();
				Assert.AreEqual(Packer.PackAndToJson(store), Packer.PackAndToJson(conductor.Store()), "シリアライズで欠損がない");
				Assert.IsFalse(conductor.HasError, "ロードを行うとリカバリー出来る");

			}
			// JumpからのBack実行
			{
				conductor.Back(1, CancellationToken.None).Wait();
				Assert.IsFalse(conductor.IsStop, "終了ブロック前に戻る");
				Assert.AreEqual("停止", text.TextBlock.Get(), "JumpはBackで戻ることが出来る");
				conductor.Back(1, CancellationToken.None).Wait();
				Assert.AreEqual("ジャンプテスト開始", text.TextBlock.Get(), "JumpはBackで戻ることが出来る");
			}
			// シーク機能
			{
				conductor.Jump("ConductorJumpTest", "jump2", CancellationToken.None).Wait();
				conductor.Update();
				Assert.AreEqual("ジャンプ先2", text.TextBlock.Get());
				Assert.IsFalse(conductor.IsStop, "終了ブロックではない");

				//シーク出来ている
				conductor.Seek("jump2", CancellationToken.None).Wait();
				conductor.Update();
				Assert.AreEqual("シーク先1", text.TextBlock.Get());


				conductor.Seek("jump2", CancellationToken.None).Wait();
				conductor.Update();
				Assert.AreEqual("シーク先2", text.TextBlock.Get());

				conductor.Seek((block) =>
				{
					return block.StopCommand != null;
				}, CancellationToken.None).Wait();

				conductor.Update();
				Assert.AreEqual("停止", text.TextBlock.Get(), "任意の場所までシーク");

				conductor.Back(1, CancellationToken.None).Wait();
				Assert.AreEqual("シーク中", text.TextBlock.Get(), "履歴には残っている");
				conductor.Back(1, CancellationToken.None).Wait();
				Assert.AreEqual("シーク中", text.TextBlock.Get(), "履歴には残っている");
				conductor.Back(1, CancellationToken.None).Wait();
				Assert.AreEqual("シーク先2", text.TextBlock.Get(), "履歴には残っている");

			}
		}

		[UnityTest, Timeout(1000)]
		public IEnumerator コマンドテスト()
		{
			TestTrigger.Trigger = null;
			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			var loader = new Loader();
			var text = new Text();
			var conductor = new Conductor(reader, loader)
			{
				Text = text
			};
			conductor.OnLoad = text.OnLoad;
			conductor.Run("CommandTest", "", CancellationToken.None).Wait();
			conductor.Update();
			{
				Assert.AreEqual("asyncを利用するとスコープ内に同期コマンドがあっても非同期実行される", text.TextBlock.Get());
				while (string.IsNullOrEmpty(TestTrigger.Trigger))
				{
					conductor.Update();
					TestSync.EndFlag = true;
					yield return null;
				}
				Assert.AreEqual("syncend", TestTrigger.Trigger);
				TestTrigger.Trigger = null;
			}
			{
				conductor.TryNext();
				conductor.Update();
				Assert.AreEqual("非同期コマンド中にブロックを終了する", text.TextBlock.Get());
				conductor.TryNext();

				Assert.AreEqual("blockfinish", TestTrigger.Trigger);
				TestTrigger.Trigger = null;
				Assert.AreEqual("トリガーが実行されている", text.TextBlock.Get());
			}
			{
				conductor.TryNext();
				Assert.IsNull(text.TextBlock, "parallelコマンドはデフォルトどれかが同期だと同期コマンドになる");
				Assert.AreEqual("parallelend", TestTrigger.Trigger, "全てのコマンドが並列実行されるで上の同期コマンドを待たない");
				TestTrigger.Trigger = null;

				while (text.TextBlock == null)
				{
					conductor.Update();
					TestSync.EndFlag = true;
					yield return null;
				}
				Assert.AreEqual("parallelを利用するとスコープ内を同時に実行できる", text.TextBlock.Get());
			}
			{
				conductor.TryNext();
				Assert.AreEqual("parallelend", TestTrigger.Trigger, "全てのコマンドが並列実行されるで上の同期コマンドを待たない");
				TestTrigger.Trigger = null;
				Assert.IsNotNull(text.TextBlock);
				Assert.AreEqual("parallel内に同期命令がない場合は非同期実行される", text.TextBlock.Get());
			}
			{
				conductor.TryNext();
				Assert.AreEqual("parallelend", TestTrigger.Trigger);
				TestTrigger.Trigger = null;
				Assert.IsNotNull(text.TextBlock);
				Assert.AreEqual("parallel内に同期命令があってもsyncフラグをfalseにすると非同期になる", text.TextBlock.Get());
			}
			{
				conductor.TryNext();
				while (text.TextBlock == null)
				{
					conductor.Update();
					TestSync.EndFlag = true;
					yield return null;
				}
				Assert.AreEqual("nestしたスコープ", text.TextBlock.Get());
			}
			{

				conductor.TryNext();
				Assert.AreEqual("ジャンプ前", text.TextBlock.Get());
				conductor.TryNext();
				// Task.Yieldを進める
				yield return null;
				conductor.Update();
				Assert.IsFalse(conductor.HasError);
				Assert.AreEqual("ジャンプコマンド", text.TextBlock.Get());
			}

		}

		[Test]
		public void エンドブロックテスト()
		{
			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			var loader = new Loader();
			var text = new Text();
			var conductor = new Conductor(reader, loader)
			{
				Text = text
			};
			conductor.OnLoad = text.OnLoad;
			conductor.Run("CommandEndBlockTest", "", CancellationToken.None).Wait();
			conductor.Update();

			Assert.AreEqual("エンドブロックコマンドテスト", text.TextBlock.Get());
			conductor.TryNext();
			Assert.IsNull(text.TextBlock);
			conductor.TryNext();
			Assert.AreEqual("デフォルトセーブされる", text.TextBlock.Get());
			conductor.Back(1, CancellationToken.None).Wait();
			Assert.IsNull(text.TextBlock);
			conductor.Back(1, CancellationToken.None).Wait();
			Assert.AreEqual("エンドブロックコマンドテスト", text.TextBlock.Get());

			conductor.TryNext();
			conductor.TryNext();
			conductor.TryNext();

			Assert.IsNull(text.TextBlock);
			conductor.TryNext();
			Assert.AreEqual("ブロックは終了するがその地点でセーブは出来ない", text.TextBlock.Get());
			conductor.Back(1, CancellationToken.None).Wait();
			Assert.AreEqual("デフォルトセーブされる", text.TextBlock.Get());
		}

		[Test]
		public void フックテスト()
		{
			TestCountCommand.Count = 0;
			TestTrigger.Trigger = null;

			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			var loader = new Loader();
			var text = new Text();
			var conductor = new Conductor(reader, loader)
			{
				Text = text
			};
			conductor.EnvDataHook.Add(new TestEnvDataProcessor
			{
				Priority = 0,
			});
			conductor.EnvDataHook.Add(new TestEnvDataProcessor
			{
				Priority = 10,
			});
			conductor.OnLoad = text.OnLoad;
			conductor.Run("EnvDataHookTest", "", CancellationToken.None).Wait();
			conductor.Update();

			Assert.AreEqual("Test1", text.TextBlock.Get());
			Assert.AreEqual(TestCountCommand.Count, 2, "Hookで二つカウントコマンドが詰まれる");
			Assert.AreEqual(TestTrigger.Trigger, "value1", "MetaData経由からデータを取り出せる");
			Assert.IsTrue(conductor.TryNext(), "シナリオを進める");

			Assert.AreEqual("Test2", text.TextBlock.Get());
			Assert.AreEqual(TestCountCommand.Count, 4, "Hookで二つカウントコマンドが詰まれる");
			Assert.IsTrue(conductor.TryNext(), "シナリオを進める");

			TestCountCommand.Count = 0;
			TestTrigger.Trigger = null;

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

			public void Unload(object obj)
			{
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

			public void Set(TextBlock text, IEnvDataHolder data)
			{
				TextBlock = text?.Clone();
			}

			public Task OnLoad(Block block, IEnvDataHolder data)
			{
				TextBlock = block?.Text?.Clone();
				return Task.FromResult(true);
			}

			public bool TryNext()
			{
				if (Next)
				{
					return true;
				}
				return false;
			}
			public void Clear()
			{
				TextBlock = null;
			}
		}

		class DummyData { }

		[TagName("test_preload", Symbol = "TEST")]
		class TestPreLoad : Command
		{
			[Argument(Required = true)]
			string m_Path = default;

			protected override void Preload(IPreLoader loader)
			{
				loader.LoadRaw<DummyData>(m_Path);
			}
		}

		[TagName("test_sync", Symbol = "TEST")]
		public class TestSync : Command
		{
			public static bool EndFlag = false;

			public override bool IsSync() => true;

			public override bool IsEnd() => EndFlag;

			protected override void Initialize()
			{
				Assert.NotNull(Get<EventBroker>());
			}

			public override void FinishBlock()
			{
				EndFlag = false;
			}

		}

		[TagName("test_prepare_wait", Symbol = "TEST")]
		public class TestPrepareWaitTime : Command
		{
			[Argument(Required = true)]
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

		[TagName("test_trigger", Symbol = "TEST")]
		public class TestTrigger : Command
		{
			public static string Trigger;

			[Argument(Required = true)]
			public string Name = default;

			protected override void Execute()
			{
				Trigger = Name;
			}

		}

		[TagName("test_error", Symbol = "TEST")]
		public class TestError : Command
		{
			protected override void Execute()
			{
				throw new Exception("Error");
			}
		}

		public class TestCountCommand : Command
		{
			public static int Count;

			protected override void Execute()
			{
				Count++;
			}
		}

		public class TestEnvDataProcessor : IEnvDataCustomProcessor
		{
			public struct PriorityEnvData : IEnvValue
			{
				public int Priority;
			}

			public int Priority { get; set; }

			public void PreUpdate(EnvDataUpdateParam param)
			{
				if (param.Data.TryGetSingle<PriorityEnvData>(out var data))
				{
					Assert.IsTrue(data.Priority > Priority, "優先順になっている");
				}
				param.Data.SetSingle(new PriorityEnvData
				{
					Priority = Priority,
				});
				param.AddCommand(new TestCountCommand());
			}

			public void PostUpdate(EnvDataUpdateParam param)
			{
				if (param.Data.TryGetSingle<PriorityEnvData>(out var data) && data.Priority == Priority)
				{
					param.AddCommand(new TestTrigger
					{
						Name = param.Meta.Get<TestMetaData>("key1").Value,
					});
					param.Data.DeleteSingle<PriorityEnvData>();
				}
			}

			public void PostJump(IMetaData meta, IEnvData data)
			{
			}
		}

	}
}