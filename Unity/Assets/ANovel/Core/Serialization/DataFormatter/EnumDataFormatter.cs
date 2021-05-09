using System;

namespace ANovel.Serialization
{
	public class EnumDataFormatter : DataFormatter
	{
		protected override object Read(Reader reader, Type type)
		{
			return Enum.Parse(type, reader.ReadString());
		}

		protected override void Write(Writer writer, object obj)
		{
			writer.WriteRefString(obj.ToString());
		}
	}


}