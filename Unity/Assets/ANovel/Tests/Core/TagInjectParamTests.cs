using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace ANovel.Core.Tests
{
	public class TagInjectParamTests
	{

		[Test]
		public void デシリアライズテスト()
		{
			var reader = new LineReader("デシリアライズテスト", "@test_inject_param x=10 y=20 z=30");
			var tag = (InjectParamTestCommand)Read(reader);
			{
				Assert.IsNotNull(tag.Test1);
				Assert.AreEqual(10, tag.Test1.ConvX, "キー設定");
				Assert.AreEqual(0, tag.Test1.PrivateX, "InjectParamでは非公開変数には設定されない");
				Assert.AreEqual(10, tag.Test1.X);
				Assert.AreEqual(20, tag.Test1.Y);
			}
			{
				Assert.IsNotNull(tag.Test2, "");
				Assert.AreEqual(10, tag.Test2.ConvX, "継承してもベースの値にセット出来る");
				Assert.AreEqual(0, tag.Test2.PrivateX);
				Assert.AreEqual(10, tag.Test2.X);
				Assert.AreEqual(20, tag.Test2.Y);
				Assert.AreEqual(30, tag.Test2.Z);
				Assert.AreEqual(111, tag.Test2.Flag, "新規インスタンスは作られない");
			}
			{
				Assert.AreEqual(10, tag.Vec1.x);
				Assert.AreEqual(20, tag.Vec1.y);
			}
			{
				Assert.AreEqual(10, tag.Vec2.x, "キー制限");
				Assert.AreEqual(0, tag.Vec2.y, "キー制限");
			}
			{
				Assert.AreNotEqual(10, tag.SkipTest.X, "SkipInjectParam属性がある場合は値が入らない");
			}
		}


		Tag Read(LineReader reader)
		{
			LineData data = default;
			Assert.IsTrue(reader.TryRead(ref data), "読み取れる");
			var ret = new List<Tag>();
			var provider = new TagProvider();
			provider.Provide(in data, ret);
			return ret.Count > 0 ? ret[0] : null;
		}

		[UnityEngine.Scripting.Preserve]
		class InjectParamTest
		{
			[UnityEngine.Scripting.Preserve]
			public int X { get; set; }
			int m_X = default;
			public int PrivateX => m_X;
			public int Y = default;

			[Argument(KeyName = "x")]
			int m_ConvX = default;
			public int ConvX => m_ConvX;
		}

		[UnityEngine.Scripting.Preserve]
		class InjectParamSkipTest
		{
			[SkipArgument]
			public int X { get; set; }
		}

		[UnityEngine.Scripting.Preserve]
		class InjectParamTestDep : InjectParamTest
		{
			public int Z = 5;

			public int Flag = default;

		}

		[TagName("test_inject_param")]
		class InjectParamTestCommand : Command
		{
			[InjectArgument]
			InjectParamTest m_Test1 = default;
			public InjectParamTest Test1 => m_Test1;
			[InjectArgument]
			public InjectParamTestDep Test2 { get; set; } = new InjectParamTestDep()
			{
				Flag = 111,
			};
			[InjectArgument]
			public InjectParamTestDep Test3 { get; set; }
			[InjectArgument]
			Vector2 m_Vec1 = default;
			public Vector2 Vec1 => m_Vec1;
			[InjectArgument(TargetKey = "x")]
			public Vector2 Vec2 { get; set; } = default;
			[InjectArgument]
			public InjectParamSkipTest SkipTest = default;

		}

	}
}