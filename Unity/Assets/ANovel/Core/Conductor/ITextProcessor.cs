namespace ANovel
{
	public interface ITextProcessor
	{
		bool IsProcessing { get; }
		void Set(TextBlock text, IEnvDataHolder data, IMetaData meta);
		bool TryNext();
		void Clear();
		string GetLocalizeKey(TextBlock text);
	}
}