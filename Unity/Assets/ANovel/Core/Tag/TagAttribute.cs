using System;
using UnityEngine.Scripting;

namespace ANovel.Core
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TagNameAttribute : PreserveAttribute
	{
		public readonly string Name;

		public readonly LineType Type;

		public int Priority { get; set; }

		public string Symbol { get; set; }

		internal TagNameAttribute(string name, LineType type)
		{
			Name = name;
			Type = type;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class TagFieldAttribute : PreserveAttribute
	{

		public bool Required { get; set; }

		public string KeyName { get; set; }

		public Type Formatter { get; set; }

		internal TagFieldAttribute() { }

	}

}