namespace ANovel.Core
{
	public readonly struct BlockLabelInfo
	{
		public readonly string FileName;
		public readonly string Name;
		public readonly int BlockIndex;
		public readonly int LineIndex;

		public BlockLabelInfo(string fileName, string name, int blockIndex, int lineIndex)
		{
			FileName = fileName;
			Name = name;
			BlockIndex = blockIndex;
			LineIndex = lineIndex;
		}
	}

}