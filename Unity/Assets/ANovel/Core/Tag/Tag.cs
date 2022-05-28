namespace ANovel.Core
{
	public abstract class Tag
	{
		public string TagName { get; private set; }

		public LineData LineData { get; private set; }

		internal void Set(string name, in LineData data)
		{
			TagName = name;
			LineData = data;
		}

	}
}