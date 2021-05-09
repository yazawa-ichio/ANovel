using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace ANovel.Core.Tests
{
	public class ParserTests
	{
		[Test]
		public void LineReaderの読み取りテスト()
		{
			var reader = new LineReader("ReaderTest", TestData.ReaderTest);

			// 空行とコメントを読み取る
			Read(reader, LineType.PreProcess, skipNoneAndComment: false);
			Read(reader, LineType.Comment, skipNoneAndComment: false);
			Read(reader, LineType.Command, skipNoneAndComment: false);
			Read(reader, LineType.SystemCommand, skipNoneAndComment: false);
			Read(reader, LineType.Label, skipNoneAndComment: false);
			Read(reader, LineType.Text, skipNoneAndComment: false);
			Read(reader, LineType.None, skipNoneAndComment: false);
			Read(reader, LineType.Command, skipNoneAndComment: false);

			LineData data = default;
			Assert.IsFalse(reader.TryRead(ref data, skipNoneAndComment: false), "読み取れない");

			// 読み取り位置をリセット
			reader.Reset();

			// 空行とコメントをスキップ
			Read(reader, LineType.PreProcess, skipNoneAndComment: true);
			Read(reader, LineType.Command, skipNoneAndComment: true);
			Read(reader, LineType.SystemCommand, skipNoneAndComment: true);
			Read(reader, LineType.Label, skipNoneAndComment: true);
			Read(reader, LineType.Text, skipNoneAndComment: true);
			Read(reader, LineType.Command, skipNoneAndComment: true);

		}

		void Read(LineReader reader, LineType type, bool skipNoneAndComment)
		{
			LineData data = default;
			Assert.IsTrue(reader.TryRead(ref data, skipNoneAndComment), "読み取れる");
			Assert.AreEqual(type, data.Type, "指定のタイプで読み取れる" + data.Line);
		}

		[Test]
		public void Tagのパラメーター読み込み()
		{
			var reader = new LineReader("KeyValueTest", TestData.KeyValueTest);
			LineData data = default;
			var param = new TagParam();
			while (reader.TryRead(ref data) && data.Type != LineType.PreProcess)
			{
				param.Set(data);
				switch (param.Name)
				{
					case "command1":
						Assert.AreEqual(0, param.Count, "パラメーターはない");
						break;
					case "command2":
						Assert.AreEqual(2, param.Count, "指定のパラメーター分ある");
						Assert.AreEqual("Value1", param["key1"], "指定のパラメーターが入る");
						Assert.AreEqual("Value2", param["key2"], "指定のパラメーターが入る");
						break;
					case "command3":
						Assert.AreEqual(2, param.Count, "指定のパラメーター分ある");
						Assert.AreEqual(null, param["key1"], "指定のパラメーターが入る");
						Assert.AreEqual(null, param["key2"], "指定のパラメーターが入る");
						break;
					case "command4":
						Assert.AreEqual(1, param.Count, "指定のパラメーター分ある");
						Assert.AreEqual("escape\"test", param["key1"], "指定のパラメーターが入る");
						break;
				}
			}

			while (reader.TryRead(ref data) && data.Type != LineType.PreProcess)
			{
				LineDataException error = null;
				try
				{
					param.Set(data);
				}
				catch (LineDataException ex)
				{
					error = ex;
				}
				Assert.IsNotNull(error, "不正な定義を防げる:" + data.Line + ":" + param.Name);
			}

		}


	}
}