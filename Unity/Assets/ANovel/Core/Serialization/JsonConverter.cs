using System.IO;
using System.Text;

namespace ANovel.Serialization
{
	public class JsonConverter
	{
		public bool Pretty { get; set; } = true;

		public string NewLine { get; set; } = "\n";

		public string Indent { get; set; } = "    ";

		public string Convert(byte[] buf)
		{
			return Convert(new Reader(buf));
		}

		public string Convert(Reader reader)
		{
			var ms = new MemoryStream();
			var encoding = new UTF8Encoding(false);
			Convert(reader, ms, encoding);
			return encoding.GetString(ms.ToArray());
		}

		public void Convert(Reader reader, Stream stream, Encoding encoding)
		{
			Convert(reader, new StreamWriter(stream, encoding));
		}

		public void Convert(Reader reader, TextWriter writer)
		{
			ConvertImpl(reader, new Writer(writer)
			{
				Pretty = Pretty,
				NewLine = NewLine,
				Indent = Indent,
			});
			writer.Flush();
		}

		void ConvertImpl(Reader reader, Writer writer)
		{
			var rawWriter = writer.RawWriter;
			switch (reader.PeekCode())
			{
				case DataTypeCode.Null:
					reader.ReadNull();
					rawWriter.Write("null");
					return;
				case DataTypeCode.Default:
					reader.ReadDefault(null);
					rawWriter.Write("{}");
					return;
				case DataTypeCode.Boolean:
					rawWriter.Write(reader.ReadBool());
					return;
				case DataTypeCode.Int32:
					rawWriter.Write(reader.ReadInt());
					return;
				case DataTypeCode.Int64:
					rawWriter.Write(reader.ReadLong());
					return;
				case DataTypeCode.Single:
					rawWriter.Write(reader.ReadFloat());
					return;
				case DataTypeCode.Double:
					rawWriter.Write(reader.ReadDouble());
					return;
				case DataTypeCode.DateTime:
					rawWriter.Write(reader.ReadDateTime());
					return;
				case DataTypeCode.String:
				case DataTypeCode.RefStringIndex:
					writer.AppendString(reader.ReadString());
					return;
				case DataTypeCode.Array:
					{
						var length = reader.ReadArray();
						if (length > 0)
						{
							writer.StartIndent("[");
							for (int i = 0; i < length; i++)
							{
								writer.AppendIndent();
								ConvertImpl(reader, writer);
								if (i < length - 1)
								{
									rawWriter.Write(",");
									writer.AppendNewLine();
								}
							}
							writer.EndIndent("]");
						}
						else
						{
							rawWriter.Write("[]");
						}
					}
					break;
				case DataTypeCode.Map:
					{
						var length = reader.ReadMap();
						if (length > 0)
						{
							writer.StartIndent("{");
							for (int i = 0; i < length; i++)
							{
								writer.AppendIndent();
								ConvertImpl(reader, writer);
								rawWriter.Write(":");
								writer.AppendScape();
								ConvertImpl(reader, writer);
								if (i < length - 1)
								{
									rawWriter.Write(",");
									writer.AppendNewLine();
								}
							}
							writer.EndIndent("}");
						}
						else
						{
							rawWriter.Write("{}");
						}
					}
					break;
				default:
					throw new System.Exception("");
			}
		}

		class Writer
		{
			public TextWriter RawWriter { get; private set; }

			public bool Pretty { get; set; } = true;

			public string NewLine { get; set; } = "\n";

			public string Indent { get; set; } = "    ";

			int m_IndentCount;

			public Writer(TextWriter writer)
			{
				RawWriter = writer;
			}

			public void AppendIndent()
			{
				if (Pretty)
				{
					for (int i = 0; i < m_IndentCount; i++)
					{
						RawWriter.Write("    ");
					}
				}
			}

			public void StartIndent(string text)
			{
				RawWriter.Write(text);
				m_IndentCount++;
				if (Pretty)
				{
					RawWriter.Write("\n");
				}
			}

			public void EndIndent(string text)
			{
				AppendNewLine();
				m_IndentCount--;
				AppendIndent();
				RawWriter.Write(text);
			}

			public void AppendNewLine()
			{
				if (Pretty)
				{
					RawWriter.Write("\n");
				}
			}

			public void AppendString(string str)
			{
				var writer = RawWriter;
				writer.Write('\"');

				foreach (var c in str)
				{
					switch (c)
					{
						case '"':
							writer.Write("\\\"");
							break;
						case '\\':
							writer.Write("\\\\");
							break;
						case '\b':
							writer.Write("\\b");
							break;
						case '\f':
							writer.Write("\\f");
							break;
						case '\n':
							writer.Write("\\n");
							break;
						case '\r':
							writer.Write("\\r");
							break;
						case '\t':
							writer.Write("\\t");
							break;
						default:
							writer.Write(c);
							break;
					}
				}

				writer.Write('\"');
			}

			public void AppendScape()
			{
				if (Pretty)
				{
					RawWriter.Write(" ");
				}
			}
		}
	}

}