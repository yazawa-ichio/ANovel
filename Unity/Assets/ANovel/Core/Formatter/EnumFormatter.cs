using System;

namespace ANovel.Core
{
	public class EnumFormatter<T> : IFormatter where T : Enum
	{
		public object Format(string value)
		{
			return Enum.Parse(typeof(T), value, true);
		}
	}
}