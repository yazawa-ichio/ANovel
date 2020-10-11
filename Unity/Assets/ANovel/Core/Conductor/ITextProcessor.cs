namespace ANovel.Core
{
	public interface ITextProcessor
	{
		bool IsProcessing { get; }
		void Set(TextBlock text);
		bool TryNext();
	}
}