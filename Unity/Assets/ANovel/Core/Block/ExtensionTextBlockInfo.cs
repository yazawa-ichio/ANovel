namespace ANovel.Core
{
	public readonly struct ExtensionTextBlockInfo
	{
		public readonly string Name;

		public readonly string Value;

		public ExtensionTextBlockInfo(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public static bool TryGet(in LineData data, out ExtensionTextBlockInfo info)
		{
			var line = data.Line.TrimEnd();
			if (line == Token.TextBlockScope)
			{
				info = new ExtensionTextBlockInfo();
				return false;
			}
			line = line.Substring(Token.TextBlockScope.Length);
			if (line[0] == Token.TextBlockScope[0])
			{
				throw new LineDataException(in data, "text block start (```) only");
			}
			var valueIndex = line.IndexOf(Token.TextBlockValueSeparater);
			if (valueIndex == 0)
			{
				throw new LineDataException(in data, "text block not found name");
			}
			else if (valueIndex > 0)
			{
				var name = line.Substring(0, valueIndex).Trim();
				var value = line.Substring(valueIndex + 1).Trim();
				if (string.IsNullOrEmpty(name))
				{
					throw new LineDataException(in data, "text block not found name");
				}
				if (string.IsNullOrEmpty(value))
				{
					throw new LineDataException(in data, "text block not found value");
				}
				info = new ExtensionTextBlockInfo(name, value);
			}
			else
			{
				info = new ExtensionTextBlockInfo(line.Trim(), null);
			}
			return true;
		}

	}
}