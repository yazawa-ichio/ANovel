using UnityEngine;

namespace ANovel.Serialization
{
	public class ColorDataFormatter : DataFormatter<Color>
	{
		public override void Write(Writer writer, Color obj)
		{
			writer.Write("#" + ColorUtility.ToHtmlStringRGBA(obj));
		}

		public override Color Read(Reader reader)
		{
			var str = reader.ReadString();
			if (str[0] == '#')
			{
				ColorUtility.TryParseHtmlString(str, out var color);
				return color;
			}
			else
			{
				ColorUtility.TryParseHtmlString("#" + str, out var color);
				return color;
			}
		}

	}

}