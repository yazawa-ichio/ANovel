
// 参考にさせていただきました。
// https://github.com/yutokun/CSV-Parser


using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ANovel.Core
{

	public static class CsvParser
	{
		public enum Delimiter
		{
			Comma,
			Tab,
		}

		/// <summary>
		/// Load CSV data from string.
		/// </summary>
		/// <param name="data">CSV string</param>
		/// <param name="delimiter">Delimiter.</param>
		/// <returns>Nested list that CSV parsed.</returns>
		public static List<List<string>> LoadFromString(string data, Delimiter delimiter = Delimiter.Comma)
		{
			return Parse(data, delimiter);
		}

		static List<List<string>> Parse(string data, Delimiter delimiter)
		{
			ConvertToCrlf(ref data);

			var sheet = new List<List<string>>();
			var row = new List<string>();
			var cell = new StringBuilder();
			var insideQuoteCell = false;
			var start = 0;

			var chars = data.ToCharArray();

			var delimiterChar = delimiter == Delimiter.Comma ? "," : "\t";
			var crlf = "\r\n";
			var oneDoubleQuot = "\"";
			var twoDoubleQuot = "\"\"";

			while (start < data.Length)
			{
				var length = start <= data.Length - 2 ? 2 : 1;
				var span = new System.ArraySegment<char>(chars, start, length);

				if (span.StartsWith(delimiterChar))
				{
					if (insideQuoteCell)
					{
						cell.Append(delimiterChar);
					}
					else
					{
						AddCell(row, cell);
					}

					start += 1;
				}
				else if (span.StartsWith(crlf))
				{
					if (insideQuoteCell)
					{
						cell.Append("\r\n");
					}
					else
					{
						AddCell(row, cell);
						AddRow(sheet, ref row);
					}

					start += 2;
				}
				else if (span.StartsWith(twoDoubleQuot))
				{
					cell.Append("\"");
					start += 2;
				}
				else if (span.StartsWith(oneDoubleQuot))
				{
					insideQuoteCell = !insideQuoteCell;
					start += 1;
				}
				else
				{
					cell.Append(span.Array[span.Offset]);
					start += 1;
				}
			}

			if (row.Count > 0)
			{
				AddCell(row, cell);
				AddRow(sheet, ref row);
			}

			return sheet;
		}

		static bool StartsWith(this System.ArraySegment<char> self, string text)
		{
			if (self.Count < text.Length)
			{
				return false;
			}
			var offset = self.Offset;
			foreach (var v in text)
			{
				if (self.Array[offset++] != v)
				{
					return false;
				}
			}
			return true;
		}

		static void AddCell(List<string> row, StringBuilder cell)
		{
			row.Add(cell.ToString());
			cell.Length = 0; // Old C#.
		}

		static void AddRow(List<List<string>> sheet, ref List<string> row)
		{
			sheet.Add(row);
			row = new List<string>();
		}

		static void ConvertToCrlf(ref string data)
		{
			data = Regex.Replace(data, @"\r\n|\r|\n", "\r\n");
		}
	}


}