using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace ANovel.Core.Tests
{
	public class BlockReaderTests
	{

		[Test]
		public void マクロインポート()
		{
			TestMacroImport("SKIP_MACRO_LOG");
			TestMacroImport("SKIP_MACRO_LOG_DEFINE", "bb");
			TestMacroImport("MACRO_LOG", "tt", "bb");
			TestMacroImport(null, "tt", "bb");
		}

		void TestMacroImport(string symbol, params string[] output)
		{
			var reader = new BlockReader(new TestDataLoader(), symbol != null ? new[] { symbol } : new string[0]);
			reader.Load("ImportMacroTest.anovel").Wait();
			var block = new Block();
			Assert.IsTrue(reader.TryRead(block));

			block.Commands.ForEach(x => (x as IExecute).Execute());

			var logs = block.Commands.OfType<TestLogCommand>().ToArray();
			for (int i = 0; i < output.Length; i++)
			{
				Assert.AreEqual(output[i], logs[i].Message, "ログの出力が不一致");
			}

			var define = block.Commands.OfType<TestMacroDefineCommand>().First();
			if (symbol == null)
			{
				Assert.AreEqual("No Define", define.Message, "ログの出力が不一致");
			}
			else
			{
				Assert.AreEqual("Define " + symbol, define.Message, "ログの出力が不一致");
			}
		}

		[Test]
		public void 依存マクロインポート()
		{
			var reader = new BlockReader(new TestDataLoader());
			reader.Load("CircleImportTest.anovel").Wait();
			var block = new Block();
			Assert.IsTrue(reader.TryRead(block));
			var log = block.Commands.OfType<TestLogCommand>().First();
			Assert.AreEqual("dep", log.Message, "依存先のマクロが利用できている");
		}

		[Test]
		public void テキストブロック()
		{
			var reader = new BlockReader(new TestDataLoader());
			reader.Load("TextBlockTest.anovel").Wait();
			var block = new Block();
			int blockIndex = 0;
			{
				Assert.IsTrue(reader.TryRead(block));
				Assert.AreEqual("開始", block.LabelInfo.Name);
				Assert.AreEqual(blockIndex++, block.LabelInfo.BlockIndex);
				Assert.AreEqual(TextBlockType.Text, block.Text.Type);
				Assert.AreEqual("一行テキスト", block.Text.Get());
			}
			{
				Assert.IsTrue(reader.TryRead(block));
				Assert.AreEqual("開始", block.LabelInfo.Name);
				Assert.AreEqual(blockIndex++, block.LabelInfo.BlockIndex, "ブロック単位でインクリメントされる");
				Assert.AreEqual(TextBlockType.Text, block.Text.Type);
				Assert.AreEqual("二行テキスト\n二行テキスト", block.Text.Get());
				Assert.AreEqual("二行テキスト\r\n二行テキスト", block.Text.Get("\r\n"), "指定の改行コードに出来る");
			}
			{
				Assert.IsTrue(reader.TryRead(block));
				Assert.AreEqual(blockIndex++, block.LabelInfo.BlockIndex, "ブロック単位でインクリメントされる");
				Assert.AreEqual(TextBlockType.Text, block.Text.Type);
				Assert.AreEqual("空白ありテキスト\n\n上が空白", block.Text.Get(), "明示的なテキストブロックでは空行も含まれる");
			}
			{
				Assert.IsTrue(reader.TryRead(block));
				Assert.AreEqual(blockIndex++, block.LabelInfo.BlockIndex, "ブロック単位でインクリメントされる");
				Assert.AreEqual(2, block.Commands.Count, "命令とコマンドが一度に取れる");
				Assert.AreEqual("命令とテキストが一度に取れる", block.Text.Get());
			}
			{
				Assert.IsTrue(reader.TryRead(block));
				Assert.AreEqual(blockIndex++, block.LabelInfo.BlockIndex, "ブロック単位でインクリメントされる");
				Assert.AreEqual(1, block.Commands.Count, "命令でテキストブロックは解除される");
				Assert.AreEqual("命令でテキストブロックは解除される", block.Text.Get());
			}
			{
				Assert.IsTrue(reader.TryRead(block));
				Assert.AreEqual(TextBlockType.Extension, block.Text.Type);
				Assert.AreEqual("test", block.Text.Extension.Name);
				Assert.AreEqual(null, block.Text.Extension.Value);
				Assert.AreEqual("特殊テキストブロック", block.Text.Get());
			}
			{
				Assert.IsTrue(reader.TryRead(block));
				Assert.AreEqual(TextBlockType.Extension, block.Text.Type);
				Assert.AreEqual("test", block.Text.Extension.Name, "空白は無視される");
				Assert.AreEqual("value", block.Text.Extension.Value, "値付きで取れる");
				Assert.AreEqual("値付き特殊テキストブロック", block.Text.Get());
			}
			{
				Assert.IsTrue(reader.TryRead(block));
				Assert.AreEqual("終了", block.LabelInfo.Name);
				Assert.AreEqual(0, block.LabelInfo.BlockIndex, "新しいラベルリセットされる");
				Assert.AreEqual("最終行テキスト", block.Text.Get());
			}
			{
				Assert.IsFalse(reader.TryRead(block), "最後まで読み取った");
				Assert.IsTrue(reader.EndOfFile, "最後まで読み取った");
			}
		}

		[Test]
		public void 不正スクリプトテスト()
		{
			CacheLineDataError("````\ntest\n```", "```は四つ並べるとエラー").Wait();
			CacheLineDataError("```\ntest\ntest\ntest\n\n\ntest", "```が閉じられていない").Wait();
			CacheLineDataError("@test_log\n*label\ntest", "ラベルは演出命令前に定義する必要がある").Wait();
			CacheLineDataError("```:test\n```", "名前がない").Wait();
			CacheLineDataError("``` :test\n```", "名前がない").Wait();
			CacheLineDataError("```test:\n```", "値がない").Wait();
			CacheLineDataError("```test : \n```", "値がない").Wait();
			CacheLineDataError("#macro name=test\ntest\n#macroend", "マクロ中にテキストブロックは入れられない").Wait();
			CacheLineDataError("#macro name=test\n*test\n#macroend", "マクロ中にラベルは入れられない").Wait();
			CacheLineDataError("#macro name=test\n#define name=test\n#macroend", "マクロ中にif以外のPreProcess命令は入れれない").Wait();
			CacheLineDataError("#macro name=test\n@test_log", "マクロが終わらない").Wait();
			CacheLineDataError("#if condition=test\n@test_log", "ifが終わらない").Wait();
			CacheLineDataError("#if condition=test not\n@test_log\n", "ifが終わらない").Wait();
			CacheLineDataError("#if condition=test not\n@test_log\n#else\n#if condition=t\n#endif", "ifが終わらない+無効ブロックのifカウント").Wait();
			CacheLineDataError("#if condition=test\n@test_log#else\n@test_log", "ifが終わらない").Wait();
			CacheLineDataError("#elseif condition=test\n@test_log", "elseifから開始出来ない").Wait();
			CacheLineDataError("#else\n@test_log", "elseから開始出来ない").Wait();
			CacheLineDataError("#endif\n@test_log", "elseから開始出来ない").Wait();
			CacheLineDataError("#if condition=test\n@test_log\n#else\n#else\n#endif", "elseから開始出来ない").Wait();
			CacheLineDataError("@not_found", "存在しない命令").Wait();
		}

		Task CacheLineDataError(string text, string message)
		{
			var reader = new BlockReader(new DummyLoader(text));
			return CacheLineDataError(reader, message);
		}

		async Task CacheLineDataError(BlockReader reader, string message)
		{
			try
			{
				await reader.Load("errortest");
				var block = new Block();
				Assert.IsFalse(reader.TryRead(block), message);
			}
			catch (LineDataException ex)
			{
				Debug.Log(message + ":" + ex.Message);
			}
		}

		interface IExecute
		{
			void Execute();
		}

		[CommandName("test_log")]
		class TestLogCommand : Command, IExecute
		{
			[CommandField]
			public string Message { get; private set; }

			public void Execute()
			{
				Debug.Log(Message);
			}
		}

		[CommandName("test_macro_define")]
		class TestMacroDefineCommand : Command, IExecute
		{
			[CommandField]
			public string Message { get; private set; }

			public void Execute()
			{
				Debug.Log(Message);
			}
		}

		class DummyLoader : IFileLoader
		{
			string m_Text;
			public DummyLoader(string text)
			{
				m_Text = text;
			}

			public Task<string> Load(string path)
			{
				return Task.FromResult(m_Text);
			}
		}


	}
}