namespace ANovel
{
	public interface ITextBlock
	{
		int LineCount { get; }
		string GetLine(int index);
		string Get(string newline);
	}

	public static class ITextBlockExtension
	{
		public static string Get(this ITextBlock self)
		{
			return self.Get("\n");
		}

		public static bool TryParseName(this ITextBlock self, string start, string end, out string keyName, out string dispName)
		{
			keyName = null;
			dispName = null;
			if (self.LineCount < 1)
			{
				return false;
			}
			var top = self.GetLine(0).Trim();
			if (top.StartsWith(start) && top.EndsWith(end))
			{
				string name = top.Substring(1, top.Length - 2);
				keyName = name;
				var split = name.IndexOf('/');
				if (split >= 0)
				{
					keyName = name.Substring(split + 1);
					dispName = name.Substring(0, split);
				}
				else
				{
					dispName = keyName;
				}
				return true;
			}
			return false;
		}


	}
}