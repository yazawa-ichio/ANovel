namespace ANovel.Core
{
	public class ArrayFormatter<T> : IFormatter
	{
		public object Format(string value)
		{
			var values = value.Split(',');
			T[] ret = new T[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				ret[i] = (T)Formatter.Format(typeof(T), values[i]);
			}
			return ret;
		}
	}

	public class StringArrayFormatter : ArrayFormatter<string>
	{
	}

}