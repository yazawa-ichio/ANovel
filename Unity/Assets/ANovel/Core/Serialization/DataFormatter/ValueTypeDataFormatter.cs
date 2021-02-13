using System;
using System.Collections.Generic;

namespace ANovel.Serialization
{
	public class ValueTypeDataFormatter : DataFormatter
	{
		public static IEnumerable<Type> TargetTypes => s_WriteMethod.Keys;

		static Dictionary<Type, Action<Writer, object>> s_WriteMethod = new Dictionary<Type, Action<Writer, object>>
		{
			{ typeof(bool), (w,v) => w.Write((bool)v) },
			{ typeof(sbyte), (w,v) => w.Write((sbyte)v) },
			{ typeof(short), (w,v) => w.Write((short)v) },
			{ typeof(int), (w,v) => w.Write((int)v) },
			{ typeof(long), (w,v) => w.Write((long)v) },
			{ typeof(byte), (w,v) => w.Write((byte)v) },
			{ typeof(ushort), (w,v) => w.Write((ushort)v) },
			{ typeof(uint), (w,v) => w.Write((uint)v) },
			{ typeof(ulong), (w,v) => w.Write((long)(ulong)v) },
			{ typeof(float), (w,v) => w.Write((float)v) },
			{ typeof(double), (w,v) => w.Write((double)v) },
			{ typeof(DateTime), (w,v) => w.Write((DateTime)v) },
			{ typeof(string), (w,v) => w.Write((string)v) },
		};

		static Dictionary<Type, Func<Reader, object>> s_ReadMethod = new Dictionary<Type, Func<Reader, object>>()
		{
			{ typeof(bool), (r) => r.ReadBool() },
			{ typeof(sbyte), (r) => (sbyte)r.ReadInt() },
			{ typeof(short), (r) => (short)r.ReadInt() },
			{ typeof(int), (r) => (int)r.ReadInt() },
			{ typeof(long), (r) =>(long) r.ReadLong() },
			{ typeof(byte), (r) => (byte)r.ReadInt() },
			{ typeof(ushort), (r) => (ushort)r.ReadInt() },
			{ typeof(uint), (r) => (uint)r.ReadLong() },
			{ typeof(ulong), (r) =>(ulong) r.ReadLong() },
			{ typeof(float), (r) => r.ReadFloat() },
			{ typeof(double), (r) => r.ReadDouble() },
			{ typeof(DateTime), (r) => r.ReadDateTime() },
			{ typeof(string), (r) => r.ReadString() },
		};

		protected override object Read(Reader reader, Type type)
		{
			return s_ReadMethod[type](reader);
		}

		protected override void Write(Writer writer, object obj)
		{
			s_WriteMethod[obj.GetType()](writer, obj);
		}

	}


}
