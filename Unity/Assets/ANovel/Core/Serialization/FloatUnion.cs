using System;
using System.Runtime.InteropServices;

namespace ANovel.Serialization
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct FloatUnion
	{
		[FieldOffset(0)]
		public float Value;

		[FieldOffset(0)]
		public byte Byte0;

		[FieldOffset(1)]
		public byte Byte1;

		[FieldOffset(2)]
		public byte Byte2;

		[FieldOffset(3)]
		public byte Byte3;

		public FloatUnion(float value)
		{
			this = default;
			Value = value;
		}

		public FloatUnion(byte[] bytes, int offset)
		{
			this = default;

			if (BitConverter.IsLittleEndian)
			{
				Byte0 = bytes[offset];
				Byte1 = bytes[offset + 1];
				Byte2 = bytes[offset + 2];
				Byte3 = bytes[offset + 3];
			}
			else
			{
				Byte0 = bytes[offset + 3];
				Byte1 = bytes[offset + 2];
				Byte2 = bytes[offset + 1];
				Byte3 = bytes[offset];
			}
		}
	}
}