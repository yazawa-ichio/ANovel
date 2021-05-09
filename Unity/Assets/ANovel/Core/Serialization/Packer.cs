using ANovel.Serialization;
using System;

namespace ANovel
{
	public static class Packer
	{
		public static bool UseRefString { get; set; } = true;

		public static byte[] Pack(object value)
		{
			Writer writer = new Writer()
			{
				UseRefString = UseRefString,
			};
			Pack(writer, value);
			var buf = writer.ToArray();
			return buf;
		}

		public static void Pack(Writer writer, object value)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else if (value is IDefaultValueSerialization def && def.IsDefault)
			{
				writer.WriteDefault();
			}
			else
			{
				var type = value.GetType();
				var nullable = Nullable.GetUnderlyingType(type);
				if (nullable != null)
				{
					type = nullable;
				}
				DataFormatter.Get(type).Write(writer, value);
			}
		}

		public static string PackAndToJson(object value, bool pretty = false)
		{
			var conv = new JsonConverter
			{
				Pretty = pretty
			};
			return conv.Convert(new Reader(Pack(value)));
		}

		public static T Unpack<T>(byte[] data)
		{
			return Unpack<T>(new Reader(data));
		}

		public static T Unpack<T>(Reader reader)
		{
			return (T)Unpack(reader, typeof(T));
		}

		public static object Unpack(Reader reader, Type type)
		{
			var nullable = Nullable.GetUnderlyingType(type);
			if (nullable != null)
			{
				type = nullable;
			}
			var code = reader.PeekCode();
			if (code == DataTypeCode.Null)
			{
				return reader.ReadNull();
			}
			else if (code == DataTypeCode.Default)
			{
				return reader.ReadDefault(type);
			}
			else
			{
				return DataFormatter.Get(type).Read(reader, type);
			}
		}

	}
}