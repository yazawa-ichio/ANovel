using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ANovel.Serialization
{


	public class Writer
	{
		byte[] m_Buf = new byte[9];
		Stream m_Stream;
		Encoding m_Encoding = Encoding.UTF8;
		List<string> m_RefString = new List<string>();

		public bool UseRefString { get; set; } = true;

		public Writer()
		{
			m_Stream = new BufferedStream(new MemoryStream(), 1024);
		}

		public void CopyTo(Stream stream)
		{
			m_Stream.Flush();
			m_Stream.Seek(0, SeekOrigin.Begin);
			m_Stream.CopyTo(stream);
		}

		public byte[] ToArray()
		{
			m_Stream.Flush();
			var buf = new byte[(int)m_Stream.Position];
			m_Stream.Seek(0, SeekOrigin.Begin);
			m_Stream.Read(buf, 0, buf.Length);
			return buf;
		}

		public void Reset()
		{
			m_Stream.Seek(0, SeekOrigin.Begin);
			m_RefString.Clear();
		}

		public void WriteNull()
		{
			m_Stream.WriteByte((byte)DataTypeCode.Null);
		}

		public void WriteDefault()
		{
			m_Stream.WriteByte((byte)DataTypeCode.Default);
		}

		public void WriteMapHeader(int length)
		{
			m_Stream.WriteByte((byte)DataTypeCode.Map);
			Write(length);
		}

		public void WriteArrayHeader(int length)
		{
			m_Stream.WriteByte((byte)DataTypeCode.Array);
			Write(length);
		}

		public void Write(bool val)
		{
			m_Buf[0] = (byte)DataTypeCode.Boolean;
			m_Buf[1] = val ? (byte)1 : (byte)0;
			m_Stream.Write(m_Buf, 0, 2);
		}

		public void Write(int val)
		{
			m_Buf[0] = (byte)DataTypeCode.Int32;
			m_Buf[1] = (byte)(val >> 0);
			m_Buf[2] = (byte)(val >> 8);
			m_Buf[3] = (byte)(val >> 16);
			m_Buf[4] = (byte)(val >> 24);
			m_Stream.Write(m_Buf, 0, sizeof(int) + 1);
		}

		public void Write(long val)
		{
			m_Buf[0] = (byte)DataTypeCode.Int64;
			m_Buf[1] = (byte)(val >> 0);
			m_Buf[2] = (byte)(val >> 8);
			m_Buf[3] = (byte)(val >> 16);
			m_Buf[4] = (byte)(val >> 24);
			m_Buf[5] = (byte)(val >> 32);
			m_Buf[6] = (byte)(val >> 40);
			m_Buf[7] = (byte)(val >> 48);
			m_Buf[8] = (byte)(val >> 56);
			m_Stream.Write(m_Buf, 0, sizeof(long) + 1);
		}

		public void Write(float val)
		{
			m_Buf[0] = (byte)DataTypeCode.Single;
			var union = new FloatUnion(val);
			if (BitConverter.IsLittleEndian)
			{
				m_Buf[1] = union.Byte0;
				m_Buf[2] = union.Byte1;
				m_Buf[3] = union.Byte2;
				m_Buf[4] = union.Byte3;
			}
			else
			{
				m_Buf[1] = union.Byte3;
				m_Buf[2] = union.Byte2;
				m_Buf[3] = union.Byte1;
				m_Buf[4] = union.Byte0;
			}
			m_Stream.Write(m_Buf, 0, sizeof(float) + 1);
		}

		public void Write(double val)
		{
			m_Buf[0] = (byte)DataTypeCode.Double;
			var union = new DoubleUnion(val);
			if (BitConverter.IsLittleEndian)
			{
				m_Buf[1] = union.Byte0;
				m_Buf[2] = union.Byte1;
				m_Buf[3] = union.Byte2;
				m_Buf[4] = union.Byte3;
				m_Buf[5] = union.Byte4;
				m_Buf[6] = union.Byte5;
				m_Buf[7] = union.Byte6;
				m_Buf[8] = union.Byte7;
			}
			else
			{
				m_Buf[1] = union.Byte7;
				m_Buf[2] = union.Byte6;
				m_Buf[3] = union.Byte5;
				m_Buf[4] = union.Byte4;
				m_Buf[5] = union.Byte3;
				m_Buf[6] = union.Byte2;
				m_Buf[7] = union.Byte1;
				m_Buf[8] = union.Byte0;
			}
			m_Stream.Write(m_Buf, 0, sizeof(double) + 1);
		}

		public void Write(string val)
		{
			if (UseRefString)
			{
				WriteRefString(val);
				return;
			}
			val = val ?? "";
			var bytes = m_Encoding.GetBytes(val);
			var length = bytes.Length;
			m_Buf[0] = (byte)DataTypeCode.String;
			m_Buf[1] = (byte)(length >> 0);
			m_Buf[2] = (byte)(length >> 8);
			m_Buf[3] = (byte)(length >> 16);
			m_Buf[4] = (byte)(length >> 24);
			m_Stream.Write(m_Buf, 0, sizeof(int) + 1);
			m_Stream.Write(bytes, 0, length);
		}

		public void Write(DateTime val)
		{
			var _val = val.ToBinary();
			m_Buf[0] = (byte)DataTypeCode.DateTime;
			m_Buf[1] = (byte)(_val >> 0);
			m_Buf[2] = (byte)(_val >> 8);
			m_Buf[3] = (byte)(_val >> 16);
			m_Buf[4] = (byte)(_val >> 24);
			m_Buf[5] = (byte)(_val >> 32);
			m_Buf[6] = (byte)(_val >> 40);
			m_Buf[7] = (byte)(_val >> 48);
			m_Buf[8] = (byte)(_val >> 56);
			m_Stream.Write(m_Buf, 0, sizeof(long) + 1);
		}

		public void WriteRefString(string val)
		{
			val = val ?? "";

			var index = m_RefString.IndexOf(val);
			if (index < 0)
			{
				index = m_RefString.Count;
				m_RefString.Add(val);

				var bytes = m_Encoding.GetBytes(val);
				var length = bytes.Length;
				m_Buf[0] = (byte)DataTypeCode.RefStringValue;
				m_Buf[1] = (byte)(length >> 0);
				m_Buf[2] = (byte)(length >> 8);
				m_Buf[3] = (byte)(length >> 16);
				m_Buf[4] = (byte)(length >> 24);
				m_Stream.Write(m_Buf, 0, sizeof(int) + 1);
				m_Stream.Write(bytes, 0, length);

			}

			m_Buf[0] = (byte)DataTypeCode.RefStringIndex;
			m_Buf[1] = (byte)(index >> 0);
			m_Buf[2] = (byte)(index >> 8);
			m_Buf[3] = (byte)(index >> 16);
			m_Buf[4] = (byte)(index >> 24);
			m_Stream.Write(m_Buf, 0, sizeof(int) + 1);
		}


	}

}
