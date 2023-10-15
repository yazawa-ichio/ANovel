namespace ANovel.GoogleApis
{
	public class SpreadSheetUtil
	{
		public static int ColumnNameToIndex(string columnName)
		{
			int result = 0;
			foreach (char c in columnName)
			{
				result *= 26;
				result += c - 'A' + 1;
			}
			return result;
		}

		public static string ColumnIndexToName(int columnIndex)
		{
			string result = "";
			while (columnIndex > 0)
			{
				int remainder = (columnIndex - 1) % 26;
				result = (char)('A' + remainder) + result;
				columnIndex = (columnIndex - remainder) / 26;
			}
			return result;
		}
	}
}