using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core.Define
{
	[System.Serializable]
	public class CompletionItemDefine
	{
		public ReplaceTagDefine[] ReplaceTag;
		public ArgumentValueDefine[] ArgumentValue;

		public static CompletionItemDefine Get(List<string> symbols)
		{
			var ret = new CompletionItemDefine();
			ret.ReplaceTag = ReplaceTagDefine.Get(symbols).ToArray();
			ret.ArgumentValue = ArgumentValueDefine.Get(symbols).ToArray();
			return ret;
		}

	}

	[System.Serializable]
	public class ReplaceTagDefine
	{
		public string LineType;
		public string RegisterTag;

		public string Key;
		public string Replace;
		public string SecondaryKey;
		public string SecondaryKeyValue;

		public string Label;

		public static IEnumerable<ReplaceTagDefine> Get(List<string> symbols)
		{
			foreach (var type in TypeUtil.GetTypesWithAttribute<ReplaceTagDefineAttribute>())
			{
				var tag = type.GetCustomAttributes(typeof(TagNameAttribute), false).FirstOrDefault() as TagNameAttribute;
				if (!string.IsNullOrEmpty(tag.Symbol) && !symbols.Contains(tag.Symbol))
				{
					continue;
				}
				var defines = type.GetCustomAttributes(typeof(ReplaceTagDefineAttribute), false).Cast<ReplaceTagDefineAttribute>();
				if (tag == null)
				{
					continue;
				}
				var token = Token.Get(type);
				foreach (var define in defines)
				{
					yield return new ReplaceTagDefine
					{
						LineType = token.ToString(),
						RegisterTag = tag.Name,
						Key = define.Key,
						Replace = define.Replace,
						SecondaryKey = define.SecondaryKey,
						SecondaryKeyValue = define.SecondaryKeyValue,
						Label = define.Label,
					};
				}
			}
		}

	}

	[System.Serializable]
	public class ArgumentValueDefine
	{
		public string LineType;
		public string RegisterTag;

		public string TargetTag;
		public string Argument;
		public string Value;
		public string SecondaryKey;
		public string SecondaryKeyValue;

		public static IEnumerable<ArgumentValueDefine> Get(List<string> symbols)
		{
			foreach (var type in TypeUtil.GetTypesWithAttribute<ArgumentValueDefineAttribute>())
			{
				var tag = type.GetCustomAttributes(typeof(TagNameAttribute), false).FirstOrDefault() as TagNameAttribute;
				if (!string.IsNullOrEmpty(tag.Symbol) && !symbols.Contains(tag.Symbol))
				{
					continue;
				}
				var defines = type.GetCustomAttributes(typeof(ArgumentValueDefineAttribute), false).Cast<ArgumentValueDefineAttribute>();
				if (tag == null)
				{
					continue;
				}
				var token = Token.Get(type);
				foreach (var define in defines)
				{
					yield return new ArgumentValueDefine
					{
						LineType = token.ToString(),
						RegisterTag = tag.Name,
						TargetTag = define.TargetTag,
						Argument = define.Argument,
						Value = define.Value,
						SecondaryKey = define.SecondaryKey,
						SecondaryKeyValue = define.SecondaryKeyValue,
					};
				}
			}
		}
	}
}