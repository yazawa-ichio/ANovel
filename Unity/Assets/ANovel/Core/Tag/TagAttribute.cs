using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace ANovel.Core
{
#if !UNITY_5_3_OR_NEWER
	public class PreserveAttribute : Attribute { }
#endif

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