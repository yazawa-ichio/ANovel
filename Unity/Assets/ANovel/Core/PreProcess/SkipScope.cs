namespace ANovel.Core
{
	public class SkipScope
	{
		public readonly int Start;
		public readonly int End;

		public SkipScope(int start, int end)
		{
			Start = start;
			End = end;
		}

		public bool IsSkip(int index)
		{
			return Start <= index && index <= End;
		}
	}

}