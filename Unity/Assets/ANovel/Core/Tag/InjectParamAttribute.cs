using System;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace ANovel
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InjectParamAttribute : PreserveAttribute
	{
		public string[] Keys { get; private set; }

		public InjectParamAttribute() { }
		public InjectParamAttribute(params string[] keys) => Keys = keys;

	}

}
