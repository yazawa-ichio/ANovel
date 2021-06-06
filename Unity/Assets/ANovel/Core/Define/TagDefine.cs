using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core.Define
{

	[Serializable]
	public class TagDefine
	{
		public string Name;
		public string Symbols;
		public string LineType;
		public string Description;
		public ArgumentDefine[] Arguments;

		public static TagDefine[] GetDefines(List<string> symbols)
		{
			return TagEntry.GetDefines(symbols).ToArray();
		}
	}

	public enum ArgumentInputType
	{
		None,
		String,
		Enum,
		Path,
		Bool,
		Number,
		Rate,
		MilliSecond,
		Color,
	}

	[Serializable]
	public class ArgumentDefine
	{
		public string Name;
		public string Description;
		public bool Required;
		public string InputType;
		public string[] InputOptions;

		public void SetDefaultInputType(Type type)
		{
			type = Nullable.GetUnderlyingType(type) ?? type;
			if (type.IsEnum)
			{
				InputType = ArgumentInputType.Enum.ToString();
				InputOptions = Enum.GetNames(type);
				return;
			}
			if (type == typeof(Millisecond))
			{
				InputType = ArgumentInputType.MilliSecond.ToString();
				return;
			}
			if (type == typeof(int) || type == typeof(float) || type == typeof(double) || type == typeof(long))
			{
				InputType = ArgumentInputType.Number.ToString();
				return;
			}
			if (type == typeof(bool))
			{
				InputType = ArgumentInputType.Bool.ToString();
				return;
			}
			if (type == typeof(UnityEngine.Color))
			{
				InputType = ArgumentInputType.Color.ToString();
				return;
			}
			if (type == typeof(string))
			{
				InputType = ArgumentInputType.String.ToString();
				return;
			}
			InputType = ArgumentInputType.None.ToString();
		}
	}


}