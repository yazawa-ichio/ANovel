using System;
using UnityEngine.Scripting;

namespace ANovel
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TagNameAttribute : PreserveAttribute
	{
		public readonly string Name;

		public int Priority { get; set; }

		public string Symbol { get; set; }

		public TagNameAttribute(string name)
		{
			Name = name;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ArgumentAttribute : PreserveAttribute
	{

		public bool Required { get; set; }

		public string KeyName { get; set; }

		public Type Formatter { get; set; }

	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class SkipArgumentAttribute : PreserveAttribute
	{
	}

}