
namespace ANovel.Core
{

	public readonly struct LineData
	{
		public readonly string Name;
		public readonly string Line;
		public readonly int Index;
		public readonly LineType Type;

		internal LineData(string name, string line, int index, LineType type)
		{
			Name = name;
			Line = line;
			Index = index;
			Type = type;
		}

	}

}