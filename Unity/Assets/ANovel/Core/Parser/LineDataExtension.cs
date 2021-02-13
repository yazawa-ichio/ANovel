using System.Collections.Generic;
using System.Text;

namespace ANovel.Core
{
	internal static class LineDataExtension
	{

		public static string ReadName(in this LineData data, out int nameEndIndex)
		{
			var token = Token.Get(data.Type);
			var start = data.Line.IndexOf(token, 0);
			if (start + 1 == data.Line.Length)
			{
				throw new LineDataException(data, "not found name");
			}
			var nameEnd = data.Line.IndexOf(Token.Empty, start);
			nameEndIndex = nameEnd;
			string name;
			if (nameEnd < 0)
			{
				name = data.Line.Substring(start + 1);
			}
			else
			{
				name = data.Line.Substring(start + 1, nameEnd - start - 1);
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new LineDataException(data, "not found name");
			}
			return name;
		}

		enum State
		{
			None,
			Key,
			KeyEnd,
			ValueStart,
			DoubleQuotationValue,
			Value,
		}

		static StringBuilder s_KeyBuilder = new StringBuilder();
		static StringBuilder s_ValueBuilder = new StringBuilder();

		public static void ReadKeyValue(in this LineData data, int startIndex, Dictionary<string, string> output)
		{
			State state = State.None;
			var key = s_KeyBuilder.Clear();
			var value = s_ValueBuilder.Clear();
			bool prevBackSlash = false;

			for (int i = startIndex; i < data.Line.Length; i++)
			{
				var c = data.Line[i];
				switch (state)
				{
					case State.None:
						if (!Token.IsEmptyOrTab(c))
						{
							key.Append(ToLowerAscii(c));
							state = State.Key;
						}
						break;
					case State.Key:
					case State.KeyEnd:
						if (Token.IsEmptyOrTab(c))
						{
							state = State.KeyEnd;
						}
						else if (c == Token.TagSplit)
						{
							state = State.ValueStart;
						}
						else if (state == State.Key)
						{
							key.Append(ToLowerAscii(c));
						}
						else if (state == State.KeyEnd)
						{
							output[key.ToString()] = null;
							key.Clear().Append(ToLowerAscii(c));
							state = State.Key;
						}
						break;
					case State.ValueStart:
						if (!Token.IsEmptyOrTab(c))
						{
							if (c == Token.DoubleQuotation)
							{
								state = State.DoubleQuotationValue;
							}
							else
							{
								state = State.Value;
								value.Append(c);
							}
						}
						break;
					case State.Value:
					case State.DoubleQuotationValue:
						if (prevBackSlash)
						{
							prevBackSlash = false;
							value.Append(c);
						}
						else if (c == Token.BackSlash)
						{
							prevBackSlash = true;
						}
						else if ((state == State.Value && Token.IsEmptyOrTab(c)) || (state == State.DoubleQuotationValue && c == Token.DoubleQuotation))
						{
							state = State.None;
							output[key.ToString()] = value.ToString();
							key.Clear();
							value.Clear();
						}
						else
						{
							value.Append(c);
						}
						break;
				}
			}
			switch (state)
			{
				case State.Key:
				case State.KeyEnd:
					if (key.Length > 0)
					{
						output[key.ToString()] = null;
						key.Clear();
					}
					break;
				case State.Value:
					output[key.ToString()] = value.ToString();
					key.Clear();
					value.Clear();
					break;
				case State.ValueStart:
				case State.DoubleQuotationValue:
					throw new LineDataException(data, $"({key}) undefine value");
				default:
					break;
			}
		}

		static char ToLowerAscii(char c)
		{
			if ('A' <= c && c <= 'Z')
			{
				c = (char)(c | 0x20);
			}
			return c;
		}


	}

}