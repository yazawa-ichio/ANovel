using System;
using System.Runtime.InteropServices;

namespace ANovel.Serialization
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct DoubleUnion
	{
		[FieldOffset(0)]
		public double Value;

		[FieldOffset(0)]
		public byte Byte0;

		[FieldOffset(1)]
		public byte Byte1;

		[FieldOffset(2)]
		public byte Byte2;

		[FieldOffset(3)]
		public byte Byte3;

		[FieldOffset(4)]
		public byte Byte4;

		[FieldOffset(5)]
		public byte Byte5;

		[FieldOffset(6)]
		public byte Byte6;

		[FieldOffset(7)]
		public byte Byte7;

		public DoubleUnion(double value)
		{
			this = default;
			Value = value;
		}

		public DoubleUnion(byte[] bytes, int offset)
		{
			this = default;

			if (BitConverter.IsLittleEndian)
			{
				Byte0 = bytes[offset];
				Byte1 = bytes[offset + 1];
				Byte2 = bytes[offset + 2];
				Byte3 = bytes[offset + 3];
				Byte4 = bytes[offset + 4];
				Byte5 = bytes[offset + 5];
				Byte6 = bytes[offset + 6];
				Byte7 = bytes[offset + 7];
			}
			else
			{
				Byte0 = bytes[offset + 7];
				Byte1 = bytes[offset + 6];
				Byte2 = bytes[offset + 5];
				Byte3 = bytes[offset + 4];
				Byte4 = bytes[offset + 3];
				Byte5 = bytes[offset + 2];
				Byte6 = bytes[offset + 1];
				Byte7 = bytes[offset];
			}
		}
	}
}
