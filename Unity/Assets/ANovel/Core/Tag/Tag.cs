namespace ANovel.Core
{
	public abstract class Tag
	{
		protected LineData LineData { get; private set; }

		internal void Set(in LineData data) => LineData = data;

	}
}