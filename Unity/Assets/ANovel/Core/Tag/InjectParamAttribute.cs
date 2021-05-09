using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace ANovel
{
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InjectParamAttribute : PreserveAttribute
	{
		public string TargetKey { get; set; }

		public string IgnoreKey { get; set; }

		public string[] TargetKeys { get; set; }

		public string[] IgnoreKeys { get; set; }

		public InjectParamAttribute() { }

		public bool TryGetTargetKeys(out HashSet<string> keys)
		{
			if (TargetKeys == null && string.IsNullOrEmpty(TargetKey))
			{
				keys = null;
				return false;
			}
			keys = new HashSet<string>();
			if (TargetKeys != null)
			{
				keys.UnionWith(TargetKeys.Select(x => x.ToLower()));
			}
			if (!string.IsNullOrEmpty(TargetKey))
			{
				keys.Add(TargetKey.ToLower());
			}
			return true;
		}

		public bool TryGetIgnoreKeys(out HashSet<string> keys)
		{
			if (IgnoreKeys == null && string.IsNullOrEmpty(IgnoreKey))
			{
				keys = null;
				return false;
			}
			keys = new HashSet<string>();
			if (IgnoreKeys != null)
			{
				keys.UnionWith(IgnoreKeys.Select(x => x.ToLower()));
			}
			if (!string.IsNullOrEmpty(IgnoreKey))
			{
				keys.Add(IgnoreKey.ToLower());
			}
			return true;
		}

	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class SkipInjectParamAttribute : PreserveAttribute
	{
	}

}