using System;
using UnityEngine;

namespace ANovel.Core
{
	public class ColorFormatter : IDefaultFormatter
	{
		public Type Target => typeof(Color);

		public object Format(string value)
		{
			if (value[0] == '#')
			{
				ColorUtility.TryParseHtmlString(value, out var color);
				return color;
			}
			else
			{
				ColorUtility.TryParseHtmlString("#" + value, out var color);
				return color;
			}
		}

	}
}