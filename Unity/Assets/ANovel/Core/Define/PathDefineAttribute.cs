using ANovel.Core.Define;
using System;
using System.Reflection;

namespace ANovel
{
	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class PathDefineAttribute : Attribute
	{
		public string Category { get; private set; }

		public string Extension { get; set; } = "*";

		public PathDefineAttribute(string category)
		{
			Category = category;
		}

		public PathDefineAttribute(object category)
		{
			Category = category.ToString();
		}

		public static bool TrySet(ArgumentDefine ret, FieldInfo field, PropertyInfo property)
		{
			PathDefineAttribute define = null;
			if (field != null && IsDefined(field, typeof(PathDefineAttribute)))
			{
				define = field.GetCustomAttribute<PathDefineAttribute>();
			}
			if (property != null && IsDefined(property, typeof(PathDefineAttribute)))
			{
				define = property.GetCustomAttribute<PathDefineAttribute>();
			}
			if (define == null)
			{
				return false;
			}
			ret.InputType = ArgumentInputType.Path.ToString();
			ret.InputOptions = new string[] { define.Category, define.Extension };
			return true;
		}
	}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class InjectPathDefineAttribute : Attribute
	{
		public string Path { get; private set; }

		public string Category { get; private set; }

		public string Extension { get; set; } = "*";

		public InjectPathDefineAttribute(string path, string category)
		{
			Path = path;
			Category = category;
		}

		public InjectPathDefineAttribute(string path, object category)
		{
			Path = path;
			Category = category.ToString();
		}

	}
}