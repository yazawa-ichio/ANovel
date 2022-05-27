using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace ANovel.Core.Tests
{
	public class FormatterTests
	{
		[Test]
		public void デフォルトフォーマッター()
		{
			Check<bool>(true);
			Check<bool?>(true);
			Check<bool>(false);
			Check<bool?>(false);
			Check<int>(100);
			Check<int?>(-100);
			Check<long>(10000000000000);
			Check<long?>(-1000000000000);
			Check<float>(100f);
			Check<float?>(-100f);
			Check<double>(10000000000000d);
			Check<double?>(-100024d);

			Check<Millisecond>(Millisecond.FromSecond(12));
			Check<Millisecond?>(Millisecond.FromSecond(2));

		}

		void Check<T>(T val)
		{
			var str = val.ToString();
			var ret = Formatter.Format(typeof(T), str);
			Assert.AreEqual(val, ret, "変換結果が同じ");
		}

		enum TestEnum
		{
			Val1,
			Val2,
		}

		[Test]
		public void 列挙型のフォーマット()
		{
			Assert.AreEqual(TestEnum.Val2, Formatter.Format(typeof(TestEnum), "Val2"));
			Assert.AreEqual(null, Formatter.Format(typeof(TestEnum?), null));
			Assert.AreEqual(TestEnum.Val2, Formatter.Format(typeof(TestEnum?), "Val2"));
		}

		[Test]
		public void カラーのフォーマット()
		{
			var formatter = Formatter.Get(typeof(ColorFormatter));
			for (int i = 0; i < 10; i++)
			{
				var color = Random.ColorHSV();
				var str = ColorUtility.ToHtmlStringRGBA(color);
				var val = (Color)formatter.Format(str);
				Assert.AreApproximatelyEqual(color.r, val.r, 1 / 256f);
				Assert.AreApproximatelyEqual(color.g, val.g, 1 / 256f);
				Assert.AreApproximatelyEqual(color.b, val.b, 1 / 256f);
				Assert.AreApproximatelyEqual(color.a, val.a, 1 / 256f);
				Assert.AreEqual(val, (Color)formatter.Format("#" + str));
			}
		}

	}
}