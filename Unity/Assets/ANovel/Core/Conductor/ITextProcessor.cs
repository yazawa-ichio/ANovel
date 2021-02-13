namespace ANovel.Core
{
	public interface ITextProcessor
	{
		bool IsProcessing { get; }
		void PreUpdate(TextBlock text, IEnvData data);
		void Set(TextBlock text);
		bool TryNext();
		void Clear();
	}

	public interface ITextProcessorRestoreHandler
	{
		void Restore(IEnvDataHolder data, TextBlock text);
	}

}