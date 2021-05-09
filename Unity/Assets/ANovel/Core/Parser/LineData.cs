
namespace ANovel.Core
{

	public readonly struct LineData
	{
		public readonly string FileName;
		public readonly string Line;
		public readonly int Index;
		public readonly LineType Type;

		internal LineData(string name, string line, int index, LineType type)
		{
			FileName = name;
			Line = line;
			Index = index;
			Type = type;
		}

	}

}