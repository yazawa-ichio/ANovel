using System;
using System.Reflection;

namespace ANovel
{
	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class DescriptionAttribute : Attribute
	{
		public readonly string Text;

		public DescriptionAttribute(string text)
		{
			Text = text;
		}

		public static string Get(Type type)
		{
			if (IsDefined(type, typeof(DescriptionAttribute)))
			{
				var attr = GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
				return attr.Text;
			}
			return type.Name;
		}

		public static string Get(MemberInfo field, MemberInfo property)
		{
			var m = field ?? property;
			if (m == null)
			{
				return "";
			}
			if (IsDefined(m, typeof(DescriptionAttribute)))
			{
				var attr = GetCustomAttribute(m, typeof(DescriptionAttribute)) as DescriptionAttribute;
				return attr.Text;
			}
			return "";
		}

	}

}