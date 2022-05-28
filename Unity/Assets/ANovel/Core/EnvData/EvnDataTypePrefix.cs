namespace ANovel.Core
{
	public static class EvnDataTypePrefix<T>
	{
		public static readonly string TypeName = typeof(T).Name;

		public static readonly string Prefix = TypeName + "@";
	}
}