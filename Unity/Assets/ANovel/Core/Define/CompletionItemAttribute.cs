using System;

namespace ANovel
{
	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ReplaceTagDefineAttribute : Attribute
	{
		public string Key { get; private set; }

		public string Replace { get; private set; }

		public string SecondaryKey { get; set; }

		public string SecondaryKeyValue { get; set; }

		public string Label { get; set; }

		public ReplaceTagDefineAttribute(string key, string replace)
		{
			Key = key;
			Replace = replace;
		}
	}

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ArgumentValueDefineAttribute : Attribute
	{
		public string TargetTag { get; private set; }

		public string Argument { get; private set; }

		public string Value { get; set; }

		public string SecondaryKey { get; set; }

		public string SecondaryKeyValue { get; set; }

		public ArgumentValueDefineAttribute(string targetTag, string argument, string value)
		{
			TargetTag = targetTag.ToLower();
			Argument = argument.ToLower();
			Value = value;
		}
	}

}