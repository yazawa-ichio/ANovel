using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace ANovel.Core.Tests
{
	public class TagTests
	{

		[Test]
		public void デシリアライズテスト()
		{
			//登録
			Formatter.Register<IntArrayFormatter>();
			var reader = new LineReader("TagTest", TestData.TagTest);
			{
				var cmd = (TestDefaultFormatter)Read(reader);
				Assert.AreEqual("test1", cmd.String, "読み取り出来ている");
				Assert.AreEqual(10, cmd.Int, "読み取り出来ている");
				Assert.AreEqual(10, cmd.Long, "読み取り出来ている");
				Assert.AreEqual(2.5f, cmd.Float, "読み取り出来ている");
				Assert.AreEqual(2.5d, cmd.Double, "読み取り出来ている");
				Assert.AreEqual(false, cmd.Bool, "読み取り出来ている");
			}
			{
				LineDataException error = null;
				try
				{
					Read(reader);
				}
				catch (LineDataException ex)
				{
					error = ex;
					Assert.IsTrue(ex.InnerException is FormatException, "フォーマットエラー");
					Assert.IsTrue(ex.Message.Contains(" format error"), "フォーマットエラーが返る");
				}
				Assert.IsNotNull(error, "フォーマットエラー");
			}
			{
				var cmd = (TestCustomFormatter)Read(reader);
				for (int i = 0; i < 3; i++)
				{
					Assert.AreEqual(i + 1, cmd.Array[i], "読み取り出来ている");
				}
				Assert.AreEqual(new Vector3(1, 2, 3), cmd.Vector3, "読み取り出来ている");
				Assert.AreEqual(45, cmd.CustomInt, "読み取り出来ている");
				Assert.AreEqual("四五", cmd.CustomIntRaw, "Keyの内容を複数デシリアライズ出来る");
			}
			Formatter.Unregister<IntArrayFormatter>();
			{
				LineDataException error = null;
				try
				{
					Read(reader);
				}
				catch (LineDataException ex)
				{
					error = ex;
				}
				Assert.IsNotNull(error, "フォーマットエラー");
			}
			{
				var cmd = (TestRequired)Read(reader);
				Assert.AreEqual(true, cmd.Required, "読み取り出来ている");
				Assert.AreEqual(true, cmd.Optional, "読み取り出来ている");
			}
			{
				var cmd = (TestRequired)Read(reader);
				Assert.AreEqual(true, cmd.Required, "読み取り出来ている");
				Assert.AreEqual(false, cmd.Optional, "オプショナルは未定義でもエラーにならない");
			}
			{
				LineDataException error = null;
				try
				{
					var cmd = (TestRequired)Read(reader);
				}
				catch (LineDataException ex)
				{
					error = ex;
					Assert.IsTrue(ex.Message.Contains(" required key"), "必須パラメーターエラーが返る");
				}
				Assert.IsNotNull(error, "Requiredがないのでエラーになる");
			}
			{
				var cmd = (TestDefaultFormatter)Read(reader);
				Assert.AreEqual("test1", cmd.String, "読み取り出来ている");
				Assert.AreEqual(20, cmd.Int, "読み取り出来ている");
				Assert.AreEqual(20, cmd.Long, "読み取り出来ている");
				Assert.AreEqual(2.5f, cmd.Float, "読み取り出来ている");
				Assert.AreEqual(2.5d, cmd.Double, "読み取り出来ている");
				Assert.AreEqual(false, cmd.Bool, "読み取り出来ている");
			}
		}

		[Test]
		public void シンボル機能テスト()
		{
			var reader = new LineReader("シンボル機能テスト", "@test_symbol");
			{
				var tag = Read(reader);
				Assert.IsNull(tag, "シンボルが定義されていないのでスキップされる");
			}
			reader.Reset();
			{
				var tag = Read(reader, symbol: TestSymbol.Symbol);
				Assert.IsNotNull(tag, "シンボルが定義されたので生成される");
				Assert.IsTrue(tag is TestSymbol, "シンボルが定義されたので生成される");
			}
		}

		[Test]
		public void マクロ機能テスト()
		{
			MacroDefine macro;
			{
				LineData data = default;
				new LineReader("マクロ機能テスト1", "@test_default_formatter key1=%macro_key1% key2=\"%macro_key2%\" key4=\"%key4%\" key5 key6=t").TryRead(ref data);
				MacroDefine dep = new MacroDefine(null, null);
				dep.Add("macro_dep", new LineData[] { data });

				new LineReader("マクロ機能テスト2", "@macro_dep macro_key1=\"macro_value_test\" macro_key2=\"%macro_key2%\" key3=\"2.5\" key1=fail").TryRead(ref data);
				macro = new MacroDefine(null, new List<MacroDefine> { dep });
				dep.Add("test_macro", new LineData[] { data });

			}

			var reader = new LineReader("マクロ機能テスト3", "@test_macro macro_key2=55\n@test_required required");
			{
				var ret = new List<Tag>();
				var provider = new TagProvider();
				provider.Macros.Add(macro);
				LineData data = default;

				{
					reader.TryRead(ref data);
					provider.Provide(in data, ret);
					var cmd = (TestDefaultFormatter)ret[0];
					Assert.AreEqual("macro_value_test", cmd.String, "読み取り出来ている");
					Assert.AreEqual(55, cmd.Int, "読み取り出来ている");
					Assert.AreEqual(0f, cmd.Float, "読み取り出来ている");
					Assert.AreEqual(true, cmd.Bool, "読み取り出来ている");
				}
				{
					ret.Clear();
					reader.TryRead(ref data);
					provider.Provide(in data, ret);

					var cmd = (TestRequired)ret[0];
					Assert.AreEqual(true, cmd.Required, "読み取り出来ている");
					Assert.AreEqual(false, cmd.Optional, "オプショナルは未定義でもエラーにならない");

				}
			}
		}

		Tag Read(LineReader reader, string symbol = null)
		{
			LineData data = default;
			Assert.IsTrue(reader.TryRead(ref data), "読み取れる");
			var ret = new List<Tag>();
			var provider = new TagProvider();
			if (symbol != null)
			{
				provider.Symbols.Add(symbol);
			}
			provider.Converters.Add(new ReplaceTagConverter());
			provider.Macros.Add(new MacroDefine(null, new List<MacroDefine>()));
			provider.Provide(in data, ret);
			return ret.Count > 0 ? ret[0] : null;
		}

		[CommandName("test_default_formatter", Priority = -1)]
		class IgnoreCommand : Command
		{

		}

		[CommandName("test_default_formatter")]
		class TestDefaultFormatter : Command
		{
			[CommandField(KeyName = "key1")]
			string m_String = default;
			public string String => m_String;

			[CommandField(KeyName = "key2")]
			int m_Int = default;
			public int Int => m_Int;

			[CommandField(KeyName = "key2")]
			long m_Long = default;
			public long Long => m_Long;

			[CommandField(KeyName = "key3")]
			float m_Float = default;
			public float Float => m_Float;

			[CommandField(KeyName = "key3")]
			double m_Double = default;
			public double Double => m_Double;

			[CommandField(KeyName = "key4")]
			bool m_Bool = default;
			public bool Bool => m_Bool;
		}

		[CommandName("test_custom_formatter")]
		class TestCustomFormatter : Command
		{
			[CommandField]
			int[] key1 = default;
			public int[] Array => key1;

			[CommandField(Formatter = typeof(Vector3Formatter))]
			Vector3 _Key2 = default;
			public Vector3 Vector3 => _Key2;

			[CommandField(Formatter = typeof(CustomIntFormatter))]
			int m_Key3 = default;
			public int CustomInt => m_Key3;

			[CommandField(KeyName = "key3")]
			string m_Key3Raw = default;
			public string CustomIntRaw => m_Key3Raw;

		}

		/// <summary>
		/// 基底にフィールドが利用できるか？
		/// </summary>
		class TestRequiredBase : Command
		{
			[CommandField(Required = false)]
			public bool Optional { get; private set; }
		}

		[CommandName("test_required")]
		class TestRequired : TestRequiredBase
		{
			[CommandField(Required = true)]
			public bool Required { get; private set; }
		}

		[CommandName("test_symbol", Symbol = TestSymbol.Symbol)]
		class TestSymbol : Command
		{
			public const string Symbol = "TEST_SYMBOL";
		}


		class IntArrayFormatter : IDefaultFormatter
		{
			public Type Target => typeof(int[]);

			public object Format(string value)
			{
				return value.Split(',').Select(x => int.Parse(x)).ToArray();
			}
		}

		class Vector3Formatter : IFormatter
		{
			public object Format(string value)
			{
				var buf = value.Split(',').Select(x => int.Parse(x)).ToArray();
				return new Vector3(buf[0], buf[1], buf[2]);
			}
		}

		class CustomIntFormatter : IFormatter
		{
			static char[] s_Index = new char[] { '〇', '一', '二', '三', '四', '五', '六', '七', '八', '九' };

			public object Format(string value)
			{
				int ret = 0;
				for (int i = 0; i < value.Length; i++)
				{
					var c = value[value.Length - i - 1];
					ret += Array.IndexOf(s_Index, c) * (int)Mathf.Pow(10, i);
				}
				return ret;
			}
		}

		class ReplaceTagConverter : IParamConverter
		{
			public void Convert(TagParam param)
			{
				if (param.Name.StartsWith("replace_"))
				{
					param.Name = param.Name.Replace("replace_", "");
				}
				if (param.TryGetValue("key2_replace", out var str) && str == null)
				{
					param["key2"] = "20";
				}
			}
		}

	}
}