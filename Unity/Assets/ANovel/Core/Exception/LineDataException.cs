using System;

namespace ANovel.Core
{
	public class LineDataException : Exception
	{
		public LineData LineData { get; private set; }

		public LineDataException(in LineData data, string message) : base(GetMessage(in data, message))
		{
			LineData = data;
		}

		public LineDataException(in LineData data, string message, Exception innerException) : base(GetMessage(in data, message), innerException)
		{
			LineData = data;
		}

		static string GetMessage(in LineData data, string message)
		{
#if DEBUG || UNITY_EDITOR
			return $"{message}\n{data.FileName}:{data.Index + 1}::{data.Line}";
#else
			return $"{message}";
#endif
		}

	}
}