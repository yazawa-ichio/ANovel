using System;

namespace ANovel.Serialization.Tests
{

	public partial class SerializationTests
	{
		public class DefaultValue : IEquatable<DefaultValue>
		{
			public bool BoolValue;
			public sbyte SbyteValue;
			public short ShortValue;
			public int IntValue;
			public long LongValue;
			public byte ByteValue;
			public ushort UshortValue;
			public uint UintValue;
			public ulong UlongValue;
			public float FloatValue;
			public double DoubleValue;
			public DateTime DateTimeValue;
			public string StringValue;

			public bool? BoolNullValue;
			public sbyte? SbyteNullValue;
			public short? ShortNullValue;
			public int? IntNullValue;
			public long? LongNullValue;
			public byte? ByteNullValue;
			public ushort? UshortNullValue;
			public uint? UintNullValue;
			public ulong? UlongNullValue;
			public float? FloatNullValue;
			public double? DoubleNullValue;
			public DateTime? DateTimeNullValue;

			public bool Equals(DefaultValue other)
			{
				return other != null &&
					   BoolValue == other.BoolValue &&
					   SbyteValue == other.SbyteValue &&
					   ShortValue == other.ShortValue &&
					   IntValue == other.IntValue &&
					   LongValue == other.LongValue &&
					   ByteValue == other.ByteValue &&
					   UshortValue == other.UshortValue &&
					   UintValue == other.UintValue &&
					   UlongValue == other.UlongValue &&
					   FloatValue == other.FloatValue &&
					   DoubleValue == other.DoubleValue &&
					   DateTimeValue == other.DateTimeValue &&
					   StringValue == other.StringValue &&
					   BoolNullValue == other.BoolNullValue &&
					   SbyteNullValue == other.SbyteNullValue &&
					   ShortNullValue == other.ShortNullValue &&
					   IntNullValue == other.IntNullValue &&
					   LongNullValue == other.LongNullValue &&
					   ByteNullValue == other.ByteNullValue &&
					   UshortNullValue == other.UshortNullValue &&
					   UintNullValue == other.UintNullValue &&
					   UlongNullValue == other.UlongNullValue &&
					   FloatNullValue == other.FloatNullValue &&
					   DoubleNullValue == other.DoubleNullValue &&
					   DateTimeNullValue == other.DateTimeNullValue;
			}

		}

		DefaultValue CreateTestValue()
		{
			return new DefaultValue()
			{
				BoolNullValue = false,
				BoolValue = true,
				SbyteValue = sbyte.MaxValue,
				SbyteNullValue = sbyte.MinValue,
				ShortValue = short.MaxValue,
				ShortNullValue = short.MinValue,
				IntNullValue = int.MaxValue,
				IntValue = int.MinValue,
				LongNullValue = long.MaxValue,
				LongValue = long.MinValue,
				FloatValue = float.Epsilon,
				FloatNullValue = float.MaxValue,
				DoubleValue = double.Epsilon,
				DoubleNullValue = double.MinValue,
				ByteValue = byte.MaxValue,
				ByteNullValue = byte.MaxValue,
				UshortNullValue = byte.MaxValue,
				UshortValue = byte.MaxValue,
				UintValue = uint.MaxValue,
				UintNullValue = ushort.MaxValue,
				UlongValue = ulong.MaxValue,
				UlongNullValue = ushort.MaxValue,
				DateTimeValue = DateTime.Now,
				DateTimeNullValue = DateTime.UtcNow,
				StringValue = "Test",
			};
		}


	}
}