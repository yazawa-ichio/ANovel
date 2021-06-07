using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core.Tests
{
	public class EnvDataTests
	{
		interface ITestEnvData
		{
			int Value1 { get; }
		}

		struct TestEnvData1 : IEnvValue, IEnvValueUpdate<int>, ITestEnvData
		{
			public int Value1 { get; set; }
			public string Value2;

			public void Update(int arg)
			{
				Value1 = arg;
			}
		}

		struct TestEnvData2 : IEnvValue, ITestEnvData
		{
			public int Value1 { get; set; }
		}

		[Test]
		public void 基本機能テスト()
		{
			EnvDataTest(new EnvData());
			var prefix = new EnvData().Prefixed<EnvDataTests>();
			EnvDataTest(prefix);
		}

		void EnvDataTest(IEnvData data)
		{
			var test = new TestEnvData1
			{
				Value1 = 5,
				Value2 = "bbbb",
			};
			Assert.IsFalse(data.Has<TestEnvData1>("Key"));
			Assert.IsFalse(data.TryGet<TestEnvData1>("Key", out _));

			data.Set("Key", test);

			Assert.IsTrue(data.Has<TestEnvData1>("Key"));

			Assert.AreEqual(test, data.Get<TestEnvData1>("Key"));

			Assert.IsTrue(data.TryGet<TestEnvData1>("Key", out var ret));

			Assert.AreEqual(test, ret);

			Assert.Throws<KeyNotFoundException>(() =>
			{
				data.Get<TestEnvData1>("NotKey");
			});

			data.Set("Key", new TestEnvData1
			{
				Value1 = 2,
				Value2 = "aaaa",
			});

			Assert.AreNotEqual(test, data.Get<TestEnvData1>("Key"));

			data.Update<TestEnvData1, int>("Key", 10);

			Assert.AreEqual(10, data.Get<TestEnvData1>("Key").Value1, "更新されている");

			data.Update<TestEnvData1>("Key", (x) =>
			{
				x.Value2 = "cccc";
				return x;
			});

			Assert.AreEqual("cccc", data.Get<TestEnvData1>("Key").Value2, "更新されている");

			data.Update<TestEnvData1>("Key", ChangeTestData);

			Assert.AreEqual(30, data.Get<TestEnvData1>("Key").Value1, "更新されている");

			data.Delete<TestEnvData1>("Key");

			Assert.IsFalse(data.Has<TestEnvData1>("Key"), "削除されている");

		}

		TestEnvData1 ChangeTestData(TestEnvData1 data)
		{
			data.Value1 = 30;
			return data;
		}

		[Test]
		public void 複数操作テスト()
		{
			EnvDataTest2(new EnvData());
			var prefix = new EnvData().Prefixed<EnvDataTests>();
			EnvDataTest2(prefix);
		}

		void EnvDataTest2(IEnvData data)
		{
			for (int i = 0; i < 10; i++)
			{
				data.Set("Key" + i, new TestEnvData1
				{
					Value1 = i,
				});
				data.Set("Key" + i, new TestEnvData2
				{
					Value1 = i + 100,
				});
			}

			Assert.AreEqual(10, data.GetKeys<TestEnvData1>().Count(), "キーの数が同じ");
			Assert.AreEqual(5, data.GetKeys<TestEnvData1>(x => x.Value1 % 2 == 0).Count(), "指定のキーを抽出");
			int count = 0;
			foreach (var kvp in data.GetAll<TestEnvData2>())
			{
				Assert.AreEqual("Key" + count, kvp.Key);
				Assert.AreEqual(count + 100, kvp.Value.Value1);
				count++;
			}

			data.DeleteAll<TestEnvData1>(x => x.Value1 < 3);

			Assert.AreEqual(7, data.GetKeys<TestEnvData1>().Count(), "削除されている");

			data.DeleteAll<TestEnvData2>();

			Assert.AreEqual(0, data.GetKeys<TestEnvData2>().Count(), "全削除されている");

			data.Set("Test", new TestEnvData2()
			{
				Value1 = 20,
			});

			Assert.AreEqual(1, data.GetKeys<TestEnvData2>().Count(), "追加");

			data.DeleteAllByInterface<ITestEnvData>((val) => val.Value1 < 5);

			Assert.AreEqual(5, data.GetKeys<TestEnvData1>().Count(), "削除されている");
			Assert.AreEqual(1, data.GetKeys<TestEnvData2>().Count(), "条件に合わないので削除されない");

			data.DeleteAllByInterface<ITestEnvData>();

			Assert.AreEqual(0, data.GetKeys<TestEnvData1>().Count(), "全削除されている");

			Assert.AreEqual(0, data.GetKeys<TestEnvData2>().Count(), "全削除されている");

		}

		[Test]
		public void 差分更新テスト()
		{
			var data = new EnvData();
			var diff = new List<EnvDataDiff>();
			var snapshot = new List<EnvDataSnapshot>();
			var randam = new Random();
			for (int i = 0; i < 100; i++)
			{
				if (i % 10 == 5)
				{
					data.Delete<TestEnvData1>("Test1");
					data.Delete<TestEnvData1>("Test2");
				}
				else
				{
					data.Update<TestEnvData1, int>("Test1", randam.Next());
					data.Update<TestEnvData1, int>("Test2", randam.Next());
					data.Update<TestEnvData1, int>("Test3", randam.Next());
				}
				diff.Add(data.Diff());
				snapshot.Add(data.Save());
			}

			var seek = new EnvData();
			var tmp = new EnvData();
			// 差分で進む
			for (int i = 0; i < 100; i++)
			{
				seek.Redo(diff[i]);
				tmp.Load(snapshot[i]);
				Assert.AreEqual(Packer.PackAndToJson(snapshot[i]), Packer.PackAndToJson(seek.Save()));
				Assert.AreEqual(Packer.PackAndToJson(snapshot[i]), Packer.PackAndToJson(tmp.Save()));
			}
			// さかのぼる
			for (int i = 100 - 1; i >= 0; i--)
			{
				tmp.Load(Packer.Unpack<EnvDataSnapshot>(Packer.Pack(snapshot[i])));
				Assert.AreEqual(Packer.PackAndToJson(tmp.Save()), Packer.PackAndToJson(data.Save()));
				data.Undo(Packer.Unpack<EnvDataDiff>(Packer.Pack(diff[i])));
			}
		}

		[Test]
		public void 差分確認()
		{
			EnvData data1 = new EnvData();
			EnvData data2 = new EnvData();

			data1.Update<TestEnvData1, int>("Key1", 1);
			data1.Update<TestEnvData1, int>("Key2", 100);

			var diff1 = data1.Diff();

			data1.Update<TestEnvData1, int>("Key1", 1);
			data1.Update<TestEnvData1, int>("Key2", 100);

			var diff2 = data1.Diff();

			Assert.AreNotEqual(Packer.PackAndToJson(diff1), Packer.PackAndToJson(diff2), "前回からの差分なので差がない");
			Assert.AreEqual("{}", Packer.PackAndToJson(diff2), "前回からの差分なので差がない");

		}

		[Test]
		public void プレフィックス()
		{
			var data = new EnvData();
			var prefix = data.Prefixed<EnvDataTests>();

			data.Set("Key", new TestEnvData1());
			prefix.Set("Key", new TestEnvData1
			{
				Value1 = 2,
			});

			Assert.AreEqual(2, data.GetKeys<TestEnvData1>().Count(), "全体から見ると二つ");
			Assert.AreEqual(1, prefix.GetKeys<TestEnvData1>().Count(), "プレフィックス付から見ると一つ");

			var holder = ((IEnvDataHolder)data).Prefixed<EnvDataTests>();

			Assert.AreEqual(1, holder.GetAll<TestEnvData1>().Count(), "ホルダー経緯で受け取る");

			Assert.IsTrue(holder.Has<TestEnvData1>("Key"));
			Assert.AreEqual(2, holder.Get<TestEnvData1>("Key").Value1);
			Assert.IsTrue(holder.TryGet<TestEnvData1>("Key", out var tmp));
			Assert.AreEqual(2, tmp.Value1);

		}

		[Test]
		public void シングルデータ()
		{
			var data = new EnvData();
			var prefix = data.Prefixed<EnvDataTests>();
			Assert.Throws<KeyNotFoundException>(() =>
			{
				data.GetSingle<TestEnvData1>();
			}, "キーがないのでエラーになる");

			var data1 = data.GetSingleOrCreate<TestEnvData1>();
			Assert.IsFalse(prefix.TryGetSingle<TestEnvData1>(out _), "自動でSetはされない");
			data1.Value1 = 50;
			data1.Value2 = "test";
			data.SetSingle(data1);
			Assert.IsTrue(prefix.TryGetSingle<TestEnvData1>(out _));
			Assert.AreEqual(data.GetSingle<TestEnvData1>(), data1, "値が更新されている");

			prefix.UpdateSingle<TestEnvData1, int>(1);
			Assert.AreNotEqual(data.GetSingle<TestEnvData1>(), data1, "Prefixつきからの更新でも同じキーで更新される");
			Assert.AreEqual(prefix.GetSingle<TestEnvData1>(), data.GetSingle<TestEnvData1>(), "Prefixつきからの更新でも同じキーで更新される");

			prefix.DeleteSingle<TestEnvData1>();

			Assert.IsFalse(prefix.TryGetSingle<TestEnvData1>(out _));

		}


	}

}