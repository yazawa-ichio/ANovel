using System;
using UnityEngine;

namespace ANovel.GoogleApis
{
	[Serializable]
	public class SpreadSheet
	{
		[SerializeField]
		string spreadsheetId;
		[SerializeField]
		Sheet[] sheets;

		public string SpreadsheetId
		{
			get => spreadsheetId;
			set => spreadsheetId = value;
		}
		public Sheet[] Sheets
		{
			get => sheets;
			set => sheets = value;
		}
	}

	[Serializable]
	public class Sheet
	{
		[SerializeField]
		SheetProperties properties;

		public SheetProperties Properties
		{
			get => properties;
			set => properties = value;
		}

	}

	[Serializable]
	public class SheetProperties
	{
		[SerializeField]
		int sheetId;
		[SerializeField]
		string title;
		[SerializeField]
		int index;
		[SerializeField]
		string sheetType;
		[SerializeField]
		GridProperties gridProperties;

		public int SheetId
		{
			get => sheetId;
			set => sheetId = value;
		}
		public string Title
		{
			get => title;
			set => title = value;
		}
		public int Index
		{
			get => index;
			set => index = value;
		}
		public string SheetType
		{
			get => sheetType;
			set => sheetType = value;
		}
		public GridProperties GridProperties
		{
			get => gridProperties;
			set => gridProperties = value;
		}
	}


	[Serializable]
	public class GridProperties
	{
		[SerializeField]
		int rowCount;
		[SerializeField]
		int columnCount;
		[SerializeField]
		int frozenRowCount;
		[SerializeField]
		int frozenColumnCount;
		[SerializeField]
		bool hideGridlines;
		[SerializeField]
		bool rowGroupControlAfter;
		[SerializeField]
		bool columnGroupControlAfter;

		public int RowCount
		{
			get => rowCount;
			set => rowCount = value;
		}
		public int ColumnCount
		{
			get => columnCount;
			set => columnCount = value;
		}
		public int FrozenRowCount
		{
			get => frozenRowCount;
			set => frozenRowCount = value;
		}
		public int FrozenColumnCount
		{
			get => frozenColumnCount;
			set => frozenColumnCount = value;
		}
		public bool HideGridlines
		{
			get => hideGridlines;
			set => hideGridlines = value;
		}
		public bool RowGroupControlAfter
		{
			get => rowGroupControlAfter;
			set => rowGroupControlAfter = value;
		}
		public bool ColumnGroupControlAfter
		{
			get => columnGroupControlAfter;
			set => columnGroupControlAfter = value;
		}
	}
}