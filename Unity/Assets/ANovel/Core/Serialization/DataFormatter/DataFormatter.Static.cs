using System;
using System.Collections.Generic;

namespace ANovel.Serialization
{
	public partial class DataFormatter
	{
		static ObjectFormatter s_ObjectFormatter = new ObjectFormatter();
		static Dictionary<Type, IDataFormatter> s_Dic = new Dictionary<Type, IDataFormatter>();

		static DataFormatter()
		{
			var def = new ValueTypeDataFormatter();
			foreach (var key in ValueTypeDataFormatter.TargetTypes)
			{
				Register(key, def);
			}
			Register<UnityEngine.Color>(new ColorDataFormatter());
			Register<Enum>(new EnumDataFormatter());
		}

		public static void Register<T>(IDataFormatter formatter)
		{
			Register(typeof(T), formatter);
		}

		public static void Register(Type type, IDataFormatter formatter)
		{
			s_Dic[type] = formatter;
		}

		public static IDataFormatter Get(Type type)
		{
			type = ConvertFormatterType(type);
			if (!s_Dic.TryGetValue(type, out var formatter))
			{
				formatter = s_ObjectFormatter;
			}
			return formatter;
		}

		public static Type ConvertFormatterType(Type type)
		{
			var nullable = Nullable.GetUnderlyingType(type);
			if (nullable != null)
			{
				type = nullable;
			}
			if (s_Dic.ContainsKey(type))
			{
				return type;
			}
			if (type.IsEnum)
			{
				return typeof(Enum);
			}
			return type;
		}

	}

}