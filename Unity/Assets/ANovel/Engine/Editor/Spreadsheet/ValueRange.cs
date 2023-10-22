using System;
using System.Collections.Generic;
using System.Text;

namespace ANovel.GoogleApis
{

	[Serializable]
	public class ValueRange
	{
		internal static ValueRange CreateFromJson(string json)
		{
			var ret = new ValueRange();
			var obj = MiniJSON.Json.Deserialize(json);
			var dic = obj as IDictionary<string, object>;
			ret.Range = dic?["range"]?.ToString();
			ret.MajorDimension = dic?["majorDimension"]?.ToString();
			if (dic.TryGetValue("values", out var _values) && _values is List<object> values)
			{
				ret.Values = new List<List<string>>(values.Count);
				foreach (var value in values)
				{
					var list = value as List<object>;
					if (list != null)
					{
						var strList = new List<string>(list.Count);
						foreach (var str in list)
						{
							strList.Add(str.ToString());
						}
						ret.Values.Add(strList);
					}
				}
			}
			return ret;
		}

		public string Range;

		public string MajorDimension = Dimension.ROWS;

		public List<List<string>> Values = new();

		static StringBuilder s_StringBuilder = new();
		internal string ToJson()
		{
			var sb = s_StringBuilder.Clear();
			sb.Append("{");
			sb.Append("\"range\":\"");
			SerializeText(Range, sb);
			sb.Append("\",");
			sb.Append("\"majorDimension\":\"").Append(MajorDimension).Append("\",");
			sb.Append("\"values\":[");
			for (int i = 0; i < Values.Count; i++)
			{
				var list = Values[i];
				sb.Append("[");
				for (int j = 0; j < list.Count; j++)
				{
					sb.Append("\"");
					SerializeText(list[j], sb);
					sb.Append("\"");
					if (j < list.Count - 1)
					{
						sb.Append(",");
					}
				}
				sb.Append("]");
				if (i < Values.Count - 1)
				{
					sb.Append(",");
				}
			}
			sb.Append("]");
			sb.Append("}");
			return sb.ToString();

			void SerializeText(string str, StringBuilder sb)
			{
				foreach (var c in str)
				{
					switch (c)
					{
						case '"':
							sb.Append("\\\"");
							break;
						case '\\':
							sb.Append("\\\\");
							break;
						case '\b':
							sb.Append("\\b");
							break;
						case '\f':
							sb.Append("\\f");
							break;
						case '\n':
							sb.Append("\\n");
							break;
						case '\r':
							sb.Append("\\r");
							break;
						case '\t':
							sb.Append("\\t");
							break;
						default:
							sb.Append(c);
							break;
					}
				}
			}
		}

	}
}