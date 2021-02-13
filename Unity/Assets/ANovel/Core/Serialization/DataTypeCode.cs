namespace ANovel.Serialization
{
	public enum DataTypeCode
	{
		Null = 1,
		Map = 2,
		Array = 3,

		Boolean = 11,
		//Byte = 12,
		//Int16 = 13,
		Int32 = 14,
		Int64 = 15,
		//UInt64 = 16,
		Single = 17,
		Double = 18,
		DateTime = 19,
		String = 20,

		Default = 31,
		RefStringValue = 32,
		RefStringIndex = 33,

	}
}
