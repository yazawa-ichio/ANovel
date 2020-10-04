namespace ANovel.Core
{
	public readonly struct BlockLabelInfo
	{
		public readonly string Name;
		public readonly int BlockIndex;
		public readonly int LineIndex;

		public BlockLabelInfo(string name, int blockIndex, int lineIndex)
		{
			Name = name;
			BlockIndex = blockIndex;
			LineIndex = lineIndex;
		}
	}

}