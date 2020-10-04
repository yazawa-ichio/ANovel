using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public interface IFormatter
	{
		object Format(string value);
	}

	public interface IDefaultFormatter : IFormatter
	{
		Type Target { get; }
	}

	public static class Formatter
	{

		static Dictionary<Type, Func<string, object>> s_Func = new Dictionary<Type, Func<string, object>>()
		{
			{ typeof(bool), (str) => str == null ? true : bool.Parse(str) },
			{ typeof(int), (str) => int.Parse(str) },
			{ typeof(long), (str) => long.Parse(str) },
			{ typeof(float), (str) => float.Parse(str) },
			{ typeof(double), (str) => double.Parse(str) },
			{ typeof(string), (str) => str },
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
			return s_Func[type](value);
		}

	}

}