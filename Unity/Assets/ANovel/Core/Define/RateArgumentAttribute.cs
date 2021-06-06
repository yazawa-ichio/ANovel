using ANovel.Core.Define;
using System;
using System.Reflection;

namespace ANovel
{
	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class RateArgumentAttribute : Attribute
	{
		public static bool TrySet(ArgumentDefine ret, FieldInfo field, PropertyInfo property)
		{
			if (field != null && IsDefined(field, typeof(PathDefineAttribute)))
			{
				ret.InputType = ArgumentInputType.Rate.ToString();
				return true;
			}
			if (property != null && IsDefined(property, typeof(PathDefineAttribute)))
			{
				ret.InputType = ArgumentInputType.Rate.ToString();
				return true;
			}
			return false;
		}
	}
}