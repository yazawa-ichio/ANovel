using System;
using System.Collections.Generic;
using System.Globalization;

namespace ANovel
{
	[UnityEngine.Scripting.Preserve]
	public interface IFormatter
	{
		object Format(string value);
	}

	[UnityEngine.Scripting.Preserve]
	public interface IDefaultFormatter : IFormatter
	{
		Type Target { get; }
	}

	public static class Formatter
	{

		static Dictionary<Type, Func<string, object>> s_Func = new Dictionary<Type, Func<string, object>>()
		{
			{ typeof(bool), (str) => str == null || bool.Parse(str) },
			{ typeof(int), (str) => int.Parse(str, CultureInfo.InvariantCulture) },
			{ typeof(long), (str) => long.Parse(str, CultureInfo.InvariantCulture) },
			{ typeof(float), (str) => float.Parse(str, CultureInfo.InvariantCulture) },
			{ typeof(double), (str) => double.Parse(str, CultureInfo.InvariantCulture) },
			{ typeof(string), (str) => str },
			{ typeof(Millisecond), (str) => new Millisecond(int.Parse(str, CultureInfo.InvariantCulture)) },
		};

		static Dictionary<Type, IFormatter> s_Instance = new Dictionary<Type, IFormatter>();

		public static IFormatter Get(Type type)
		{
			if (!s_Instance.TryGetValue(type, out var formatter))
			{
				s_Instance[type] = formatter = (IFormatter)Activator.CreateInstance(type);
			}
			return formatter;
		}

		public static void Register(IDefaultFormatter formatter)
		{
			s_Func[formatter.Target] = formatter.Format;
			if (formatter.Target.IsValueType)
			{
				var nullable = typeof(Nullable<>).MakeGenericType(formatter.Target);
				s_Func[nullable] = formatter.Format;
			}
		}

		public static void Register<T>() where T : IDefaultFormatter, new()
		{
			Register(new T());
		}

		public static void Unregister(IDefaultFormatter formatter)
		{
			s_Func.Remove(formatter.Target);
		}

		public static void Unregister<T>() where T : IDefaultFormatter, new()
		{
			Unregister(new T());
		}

		public static object Format(Type type, string value)
		{
			if (s_Func.TryGetValue(type, out var func))
			{
				return func(value);
			}
			if (type.IsEnum)
			{
				return Enum.Parse(type, value, true);
			}
			var nullable = Nullable.GetUnderlyingType(type);
			if (nullable != null)
			{
				if (s_Func.TryGetValue(nullable, out func))
				{
					s_Func[type] = func;
				}
				else if (nullable.IsEnum)
				{
					if (value != null)
					{
						return Enum.Parse(nullable, value, true);
					}
					return null;
				}
			}
			return s_Func[type](value);
		}

	}

}