using ANovel.Core;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ANovel.Serialization.Tests
{

	public partial class SerializationTests
	{
		public enum TestEnum
		{
			None,
			Value1,
			Value2,
		}

		public class NestTest : IEquatable<NestTest>
		{
			public string Message { get; set; }
			public int Num { get; set; }
			public TestEnum TestEnum;
			public NestTest Nest;

			public bool Equals(NestTest other)
			{
				return other != null &&
					   Message == other.Message &&
					   Num == other.Num &&
					   EqualityComparer<NestTest>.Default.Equals(Nest, other.Nest);
			}
		}

		[Serializable]
		public struct StructTest : IEquatable<StructTest>
		{
			public int Num;
			public StructNestTest Nest;
			public StructEmpty Empty;

			public bool Equals(StructTest other)
			{
				return Num == other.Num &&
					   Nest.Equals(other.Nest);
			}
		}

		[Serializable]
		public struct StructNestTest : IEquatable<StructNestTest>
		{
			public int[] Value;

			public bool Equals(StructNestTest other)
			{
				if (Value == other.Value)
				{
					return true;
				}
				if (Value == null || other.Value == null || Value.Length != other.Value.Length)
				{
					return false;
				}
				for (int i = 0; i < Value.Length; i++)
				{
					if (Value[i] != other.Value[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		[Serializable]
		public struct StructEmpty : IEquatable<StructEmpty>
		{
			public bool Equals(StructEmpty other) => true;
		}

		void CheckEqual<T>(T data)
		{
			{
				Packer.UseRefString = true;
				var buf = Packer.Pack(data);
				UnityEngine.Debug.Log(new JsonConverter().Convert(buf));
				var ret = Packer.Unpack<T>(buf);
				Assert.AreEqual(data, ret);
				var ms = new MemoryStream(buf);
				var reader = new Reader(ms);
				reader.ReadSkip();
				Assert.AreEqual(buf.Length, ms.Position);
			}
			{
				Packer.UseRefString = false;
				var buf = Packer.Pack(data);
				UnityEngine.Debug.Log(new JsonConverter().Convert(buf));
				var ret = Packer.Unpack<T>(buf);
				Assert.AreEqual(data, ret);
				var ms = new MemoryStream(buf);
				var reader = new Reader(ms);
				reader.ReadSkip();
				Assert.AreEqual(buf.Length, ms.Position);
			}
		}

		void CheckDicEqual<T>(T data) where T : IDictionary
		{
			{
				Packer.UseRefString = true;
				var buf = Packer.Pack(data);
				UnityEngine.Debug.Log(new JsonConverter().Convert(buf));
				var ret = Packer.Unpack<T>(buf);
				Assert.AreEqual(data.Count, ret.Count);
				foreach (var key in data.Keys)
				{
					Assert.AreEqual(data[key], ret[key]);
				}
				var ms = new MemoryStream(buf);
				var reader = new Reader(ms);
				reader.ReadSkip();
				Assert.AreEqual(buf.Length, ms.Position);
			}
			{
				Packer.UseRefString = false;
				var buf = Packer.Pack(data);
				UnityEngine.Debug.Log(new JsonConverter().Convert(buf));
				var ret = Packer.Unpack<T>(buf);
				Assert.AreEqual(data.Count, ret.Count);
				foreach (var key in data.Keys)
				{
					Assert.AreEqual(data[key], ret[key]);
				}
				var ms = new MemoryStream(buf);
				var reader = new Reader(ms);
				reader.ReadSkip();
				Assert.AreEqual(buf.Length, ms.Position);
			}
		}

		void CheckJsonEqual(object data)
		{
			Assert.AreEqual(JsonUtility.ToJson(data).Replace("\r\n", "\n").Trim(), Packer.PackAndToJson(data).Replace("\r\n", "\n").Trim());
			Assert.AreEqual(JsonUtility.ToJson(data, prettyPrint: true).Replace("\r\n", "\n"), Packer.PackAndToJson(data, pretty: true));
		}

		[Test]
		public void 基本データ型のシリアライズ()
		{
			CheckEqual(new DefaultValue());
			CheckEqual(CreateTestValue());
		}

		[Test]
		public void 指定型のDataFormatterでシリアライズ()
		{
			CheckEqual(new Color());
			CheckEqual<Color?>(null);
			CheckEqual(Color.red);
			CheckEqual(Color.black);

			CheckEqual(TestEnum.Value1);
			CheckEqual(TestEnum.Value2);
			CheckEqual<TestEnum?>(null);

		}

		[Test]
		public void 内部に値型以外を持つケース()
		{
			CheckEqual(new NestTest
			{
				Message = "test",
				Num = 5,
				Nest = new NestTest
				{
					Message = "Nest",
					Num = 6,
					TestEnum = TestEnum.Value2,
				},
			});
			CheckEqual(new StructTest
			{
				Num = 22,
			});
			CheckEqual(new StructTest
			{
				Num = int.MinValue,
				Nest = new StructNestTest
				{
					Value = new[] { 2, 4, -5, -8888 }
				},
			});
		}

		[Test]
		public void Json出力()
		{
			var test = new StructTest
			{
				Num = int.MinValue,
				Nest = new StructNestTest
				{
					Value = new[] { -2, -4, 5, 8888 }
				},
			};
			CheckEqual(test);
			CheckJsonEqual(test);
			test = new StructTest
			{
				Num = 55555,
				Nest = new StructNestTest
				{
					Value = Array.Empty<int>()
				}
			};
			CheckEqual(test);
			CheckJsonEqual(test);
		}

		[Test]
		public void 辞書()
		{
			{
				var dic = new Dictionary<string, string>();
				dic["test"] = "tst";
				dic["b"] = "b";
				dic["nn"] = null;
				CheckDicEqual(dic);
			}
			{
				var dic = new Dictionary<string, int>();
				dic["test"] = 5;
				dic["b"] = 32;
				dic["nn"] = 77;
				CheckDicEqual(dic);
			}
			{
				var dic = new Dictionary<int, string>();
				dic[5] = "---";
				dic[7] = null;
				dic[88] = "bbbbb";
				CheckDicEqual(dic);
			}
		}

		class TypeSerializedObject : ICustomMapSerialization, IEquatable<TypeSerializedObject>, IDefaultValueSerialization
		{
			public int Length => Value != null ? 1 : 0;

			public bool IsDefault => Value == null;

			public object Value;

			public void Write(Writer writer)
			{
				if (Value != null)
				{
					writer.Write(TypeUtil.GetTypeName(Value.GetType()));
					Packer.Pack(writer, Value);
				}
			}

			public void Read(int length, Reader reader)
			{
				if (length == 0) return;
				var type = TypeUtil.GetType(reader.ReadString());
				Value = Packer.Unpack(reader, type);
			}

			public bool Equals(TypeSerializedObject other)
			{
				return other != null &&
					   EqualityComparer<object>.Default.Equals(Value, other.Value);
			}
		}

		class CustomArrayTest : ICustomArraySerialization, IEquatable<CustomArrayTest>
		{
			public int Length => 3;

			public int Value1;
			public double Value2;
			public string Value3;

			public void Write(Writer writer)
			{
				writer.Write(Value1);
				writer.Write(Value2);
				Packer.Pack(writer, Value3);
			}

			public void Read(int length, Reader reader)
			{
				Value1 = reader.ReadInt();
				Value2 = reader.ReadDouble();
				Value3 = Packer.Unpack<string>(reader);
			}

			public bool Equals(CustomArrayTest other)
			{
				return other != null &&
					   Value1 == other.Value1 &&
					   Value2 == other.Value2 &&
					   Value3 == other.Value3;
			}
		}

		[Test]
		public void カスタムシリアライズ()
		{

			CheckEqual(new TypeSerializedObject());

			CheckEqual(new TypeSerializedObject
			{
				Value = true,
			});
			CheckEqual(new TypeSerializedObject
			{
				Value = TestEnum.Value1,
			});
			CheckEqual(new TypeSerializedObject
			{
				Value = "Test",
			});
			CheckEqual(new TypeSerializedObject
			{
				Value = 236.256,
			});
			CheckEqual(new CustomArrayTest
			{
				Value1 = 467,
				Value2 = 0.4555d,
				Value3 = "bbbb"
			});
			CheckEqual(new CustomArrayTest
			{
				Value1 = -55555,
				Value2 = 22222222,
			});
		}

		[Test]
		public void 読み書き()
		{
			Writer writer = new Writer();
			writer.WriteNull();
			writer.Reset();
			Assert.AreEqual(0, writer.ToArray().Length);
			writer.Write(true);
			writer.Write(123);
			writer.Write(long.MaxValue);
			var ms = new MemoryStream();
			writer.CopyTo(ms);
			Assert.AreEqual(writer.ToArray().Length, ms.ToArray().Length);
			ms.Seek(0, SeekOrigin.Begin);
			Reader reader = new Reader(ms);
			Assert.AreEqual(true, reader.ReadBool());
			Assert.AreEqual(123, reader.ReadInt());
			Assert.AreEqual(long.MaxValue, reader.ReadLong());
		}
	}
}