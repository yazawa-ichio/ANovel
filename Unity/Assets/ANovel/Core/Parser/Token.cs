﻿using System;

namespace ANovel.Core
{
	public static class Token
	{
		public static string[] NewLine = new[] { "\r\n", "\n", "\r" };

		public const char Comment = ';';
		public const char PreProcess = '#';
		public const char Label = '*';
		public const char Command = '@';
		public const char SystemCommand = '&';

		public const char Tab = '\t';
		public const char Empty = ' ';

		public const char TagSplit = '=';
		public const char DoubleQuotation = '\"';
		public const char BackSlash = '\\';

		public const string TextBlockScope = "```";
		public const char TextBlockValueSeparater = ':';

		public static bool IsEmptyOrTab(char c)
		{
			return c == Empty || c == Tab;
		}

		public static char Get(LineType type)
		{
			switch (type)
			{
				case LineType.Comment:
					return Comment;
				case LineType.PreProcess:
					return PreProcess;
				case LineType.Label:
					return Label;
				case LineType.Command:
					return Command;
				case LineType.SystemCommand:
					return SystemCommand;
			}
			throw new Exception($"not found token {type}");
		}

	}
}