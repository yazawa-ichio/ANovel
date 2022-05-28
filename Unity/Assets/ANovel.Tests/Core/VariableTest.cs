using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core.Tests
{
	public class VariableTest
	{
		class Loader : IScenarioLoader
		{
			string m_Text;
			public Loader(string text)
			{
				m_Text = text;
			}

			public void Dispose() { }

			public Task<string> Load(string path, CancellationToken token)
			{
				return Task.FromResult(m_Text);
			}
		}

		[Test]
		public void 変数の設定()
		{
			Assert.AreEqual(0, Set("&val name=test value=0").Variables.Get("test"));
			Assert.AreEqual(false, Set("&val name=test value=0").GlobalVariables.Has("test"));
			Assert.AreEqual(2, Set("&val name=test value=2 global").GlobalVariables.Get("test"));
			Assert.AreEqual(false, Set("&val name=test value=2 global").Variables.Has("test"));

			Assert.AreEqual(false, Set("&val name=test value=false type=Bool").Variables.Get("test"));
			Assert.AreEqual(56, Set("&val name=test value=56 type=Int").Variables.Get("test"));

			Assert.AreEqual(5.4321d, Set("&val name=test value=5.4321").Variables.Get("test"));
			Assert.AreEqual("bbb", Set("&val name=test value=bbb").Variables.Get("test"));
			Assert.AreEqual(true, Set("&val name=test value=true").Variables.Get("test"));
			Assert.AreEqual(false, Set("&val name=test value=false").Variables.Get("test"));
			Assert.AreEqual(5.0d, Set("&val name=test value=5 type=Real").Variables.Get("test"));
			Assert.AreEqual("true", Set("&val name=test value=true type=String").Variables.Get("test"));
			ErrorSet("&val name=test value=5.4321 type=Int");
			ErrorSet("&val name=test value=bbb type=Int");
			ErrorSet("&val name=test value=bbb type=Bool");
			ErrorSet("&val name=test value=1 type=Bool");
			ErrorSet("@test_log message=$\"{test}\"");


			Assert.AreEqual(10, Set("&val name=test value=true\n&val name=test value=10").Variables.Get("test"), "上書き");
			Assert.AreEqual(false, Set("&val name=test value=true\n&val_del name=test").Variables.TryGetValue("test", out _), "削除");
			Assert.AreEqual(false, Set("&val_del name=test").Variables.TryGetValue("test", out _), "削除");


			Assert.AreEqual(1, Set("&val_add name=test").Variables.Get("test"), "追加");
			Assert.AreEqual(2, Set("&val_add name=test\n&val_add name=test").Variables.Get("test"), "追加");
			Assert.AreEqual(10, Set("&val_add name=test value=10").Variables.Get("test"), "追加");
			Assert.AreEqual(-15, Set("&val_add name=test value=-15").Variables.Get("test"), "追加");
			ErrorSet("&val_add name=test allow_empty=false");
			ErrorSet("&val name=test value=5.4321\n&val_add name=test");


			Assert.AreEqual(true, Set("&flag name=test").Variables.Get("test"), "フラグ");
			Assert.AreEqual(true, Set("&flag name=test on").Variables.Get("test"), "フラグ");
			Assert.AreEqual(false, Set("&flag name=test on=false").Variables.Get("test"), "フラグ");
			Assert.AreEqual(false, Set("&flag name=test off").Variables.Get("test"), "フラグ");
			Assert.AreEqual(true, Set("&flag name=test off=false").Variables.Get("test"), "フラグ");
			ErrorSet("&flag name=test on off");

			Assert.Throws<KeyNotFoundException>(() =>
			{
				Set("&flag name=test").Variables.Get("test_error");
			});
		}

		[Test]
		public void 変数の削除()
		{
			{
				var variables = Set("&val_add name=ab_cd\n&val_add name=ab_cdf\n&val_add name=ac_cd\n&val_del_all").Variables;
				Assert.IsFalse(variables.Has("ab_cd"));
				Assert.IsFalse(variables.Has("ab_cdf"));
				Assert.IsFalse(variables.Has("ac_cd"));
			}
			{
				var variables = Set("&val_add name=ab_cd\n&val_add name=ab_cdf\n&val_add name=ac_cd\n&val_del_all prefix=ab").Variables;
				Assert.IsFalse(variables.Has("ab_cd"));
				Assert.IsFalse(variables.Has("ab_cdf"));
				Assert.IsTrue(variables.Has("ac_cd"));
			}
			{
				var variables = Set("&val_add name=ab_cd global\n&val_add name=ab_cdf global\n&val_add name=ac_cd global\n&val_del_all suffix=cd global").GlobalVariables;
				Assert.IsFalse(variables.Has("ab_cd"));
				Assert.IsTrue(variables.Has("ab_cdf"));
				Assert.IsFalse(variables.Has("ac_cd"));
			}
			{
				var variables = Set("&val_add name=ab_cd\n&val_add name=ab_cdf\n&val_add name=ac_cdb\n&val_del_all contains=b_").Variables;
				Assert.IsFalse(variables.Has("ab_cd"));
				Assert.IsFalse(variables.Has("ab_cdf"));
				Assert.IsTrue(variables.Has("ac_cdb"));
			}
			{
				ErrorSet("&val_add name=test\n@test_log message=$\"{test}\"\n&val_del name=test\n@test_log message=$\"{test}\"");
			}
		}

		[Test]
		public void 評価関数テスト()
		{
			Assert.AreEqual(21d, Set("&val_eval name=test value=(1+1)+(2*10)-1").Variables.Get("test"));
			Assert.AreEqual(21, Set("&val_eval name=test value=(1+1)+(2*10)-1 type=Int").Variables.Get("test"));
			Assert.AreEqual("21", Set("&val_eval name=test value=(1+1)+(2*10)-1 type=String").Variables.Get("test"));
			Assert.AreEqual(false, Set("&val_eval name=test value=(1+1)+(2*10)-1 type=Bool").Variables.Get("test"));
			ErrorSet("&val_eval name=test value=\"a\"");
			ErrorSet("&val_eval name=test value=\"1**2\"");
		}

		[Test]
		public void 変数の保存()
		{
			{
				var buf = Set("&val_add name=test").Variables.Save();
				Assert.AreEqual(2, Set("&val_add name=test", buf).Variables.Get("test"));
				Assert.AreEqual(2, Set("&val_add name=test", buf).Variables.Get("test"));
				Assert.AreEqual(1, Set("", buf).Variables.Get("test"));
			}
			{
				var buf = Set(@"
&val name=test1 value=12
&val name=test2 value=12.34
&val name=test3 value=true
&val name=test4 value=1b2b3
				").Variables.Save();
				var val = Set("", buf).Variables;
				Assert.AreEqual(12, val.Get("test1"));
				Assert.AreEqual(12.34, val.Get("test2"));
				Assert.AreEqual(true, val.Get("test3"));
				Assert.AreEqual("1b2b3", val.Get("test4"));
			}
		}

		void ErrorSet(string value)
		{
			Assert.Throws<LineDataException>(() =>
			{
				Set(value);
			});
		}

		IEvaluator Set(string value, byte[] save = null)
		{
			BlockReader reader = new BlockReader(new Loader(value), new string[] { "TEST" });
			if (save != null)
			{
				reader.Evaluator.Variables.Load(save);
			}
			reader.Load("", CancellationToken.None).Wait();
			Assert.IsTrue(reader.TryRead(out var block));
			{
				foreach (var cmd in block.Commands.OfType<ICommand>())
				{
					cmd.Execute();
				}
			}
			return reader.Evaluator;
		}


		[Test]
		public void 変数展開テスト()
		{
			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			reader.Evaluator.SetEnvData(new EnvData());
			reader.Load("VariableTest", CancellationToken.None).Wait();
			while (reader.TryRead(out var block))
			{
				if (block.StopCommand != null)
				{
					break;
				}
				foreach (var cmd in block.Commands)
				{
					cmd.Execute();
				}

				var logs = block.Commands.OfType<TestLogCommand>().ToArray();
				for (int i = 0; i < logs.Length; i++)
				{
					Assert.AreEqual(block.Text.GetLine(i), logs[i].Message, "ログの出力が不一致" + logs[i].LineData.Index + ":" + logs[i].LineData.Line);
				}
				Assert.AreEqual(block.Text.LineCount, logs.Length, "行数が不一致");
			}
		}

	}
}