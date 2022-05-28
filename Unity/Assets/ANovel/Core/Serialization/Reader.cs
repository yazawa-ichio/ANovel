using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ANovel.Serialization
{

	public class Reader
	{
		byte[] m_Buf = new byte[9];
		Stream m_Stream;
		Encoding m_Encoding = Encoding.UTF8;
		bool m_Peek;
		DataTypeCode m_Code;
		List<string> m_RefString = new List<string>();

		public Reader(byte[] buf)
		{
			Reset(new MemoryStream(buf));
		}

		public Reader(Stream stream)
		{
			Reset(stream);
		}

		public void Reset(Stream stream)
		{
			m_Stream = stream;
			m_RefString.Clear();
		}

		void ReadPrepare(DataTypeCode code)
		{
			if (!m_Peek)
			{
				PeekCode();
			}
			if (m_Code != code)
			{
				throw new Exception($"TypeCode Error {code} cur{m_Code}");
			}
			m_Peek = false;
		}

		public DataTypeCode PeekCode()
		{
			if (m_Peek)
			{
				return m_Code;
			}
			m_Peek = true;
			m_Code = (DataTypeCode)m_Stream.ReadByte();
			if (m_Code == DataTypeCode.RefStringValue)
			{
				ReadRefStringValue();
				return PeekCode();
			}
			return m_Code;
		}

		void ReadRefStringValue()
		{
			ReadPrepare(DataTypeCode.RefStringValue);
			m_Stream.Read(m_Buf, 0, sizeof(int));
			var length = (m_Buf[0] << 0) | (m_Buf[1] << 8) | (m_Buf[2] << 16) | (m_Buf[3] << 24);
			byte[] bytes = new byte[length];
			m_Stream.Read(bytes, 0, length);
			var str = m_Encoding.GetString(bytes).Trim((char)0);
			m_RefString.Add(str);
		}

		public object ReadNull()
		{
			ReadPrepare(DataTypeCode.Null);
			return null;
		}

		public object ReadDefault(Type type)
		{
			ReadPrepare(DataTypeCode.Default);
			if (type != null)
			{
				return Activator.CreateInstance(type);
			}
			else
			{
				return null;
			}
		}

		public int ReadMap()
		{
			ReadPrepare(DataTypeCode.Map);
			return ReadInt();
		}

		public int ReadArray()
		{
			ReadPrepare(DataTypeCode.Array);
			return ReadInt();
		}

		public bool ReadBool()
		{
			ReadPrepare(DataTypeCode.Boolean);
			return m_Stream.ReadByte() == 1;
		}

		public int ReadInt()
		{
			ReadPrepare(DataTypeCode.Int32);
			m_Stream.Read(m_Buf, 0, sizeof(int));
			return (m_Buf[0] << 0) | (m_Buf[1] << 8) | (m_Buf[2] << 16) | (m_Buf[3] << 24);
		}

		public long ReadLong()
		{
			ReadPrepare(DataTypeCode.Int64);
			m_Stream.Read(m_Buf, 0, sizeof(long));
			return ((long)m_Buf[0] << 0) | ((long)m_Buf[1] << 8) | ((long)m_Buf[2] << 16) | ((long)m_Buf[3] << 24) |
				((long)m_Buf[4] << 32) | ((long)m_Buf[5] << 40) | ((long)m_Buf[6] << 48) | ((long)m_Buf[7] << 56);
		}

		public float ReadFloat()
		{
			ReadPrepare(DataTypeCode.Single);
			m_Stream.Read(m_Buf, 0, sizeof(float));
			return new FloatUnion(m_Buf, 0).Value;
		}

		public double ReadDouble()
		{
			ReadPrepare(DataTypeCode.Double);
			m_Stream.Read(m_Buf, 0, sizeof(double));
			return new DoubleUnion(m_Buf, 0).Value;
		}

		public DateTime ReadDateTime()
		{
			ReadPrepare(DataTypeCode.DateTime);
			m_Stream.Read(m_Buf, 0, sizeof(long));
			var val = ((long)m_Buf[0] << 0) | ((long)m_Buf[1] << 8) | ((long)m_Buf[2] << 16) | ((long)m_Buf[3] << 24) |
				((long)m_Buf[4] << 32) | ((long)m_Buf[5] << 40) | ((long)m_Buf[6] << 48) | ((long)m_Buf[7] << 56);
			return DateTime.FromBinary(val);
		}

		public string ReadString()
		{
			var peek = PeekCode();
			switch (peek)
			{
				case DataTypeCode.String:
					{
						ReadPrepare(DataTypeCode.String);
						m_Stream.Read(m_Buf, 0, sizeof(int));
						var length = (m_Buf[0] << 0) | (m_Buf[1] << 8) | (m_Buf[2] << 16) | (m_Buf[3] << 24);
						byte[] bytes = new byte[length];
						m_Stream.Read(bytes, 0, length);
						var str = m_Encoding.GetString(bytes).Trim((char)0);
						return str;
					}
				case DataTypeCode.RefStringIndex:
					{
						ReadPrepare(DataTypeCode.RefStringIndex);
						m_Stream.Read(m_Buf, 0, sizeof(int));
						var index = (m_Buf[0] << 0) | (m_Buf[1] << 8) | (m_Buf[2] << 16) | (m_Buf[3] << 24);
						return m_RefString[index];
					}
				default:
					throw new Exception($"TypeCode Error {m_Code}");
			}
		}

		public void ReadSkip()
		{
			switch (PeekCode())
			{
				case DataTypeCode.Null:
					ReadNull();
					return;
				case DataTypeCode.Default:
					ReadDefault(null);
					return;
				case DataTypeCode.Boolean:
					ReadBool();
					return;
				case DataTypeCode.Int32:
					ReadInt();
					return;
				case DataTypeCode.Int64:
					ReadLong();
					return;
				case DataTypeCode.Single:
					ReadFloat();
					return;
				case DataTypeCode.Double:
					ReadDouble();
					return;
				case DataTypeCode.String:
				case DataTypeCode.RefStringIndex:
					ReadString();
					return;
				case DataTypeCode.DateTime:
					ReadDateTime();
					return;
				case DataTypeCode.Array:
					{
						var length = ReadArray();
						for (int i = 0; i < length; i++)
						{
							ReadSkip();
						}
					}
					return;
				case DataTypeCode.Map:
					{
						var length = ReadMap();
						for (int i = 0; i < length; i++)
						{
							ReadSkip();
							ReadSkip();
						}
					}
					return;
				default:
					throw new Exception("Skip fail");
			}
		}

	}

}