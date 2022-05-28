namespace ANovel
{
	public interface ITextProcessor
	{
		bool IsProcessing { get; }
		void Set(TextBlock text, IEnvDataHolder data);
		bool TryNext();
		void Clear();
	}
}