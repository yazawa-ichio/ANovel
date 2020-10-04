namespace ANovel.Core
{
	public interface IImportPreProcess
	{
		string Path { get; }
		void Import(PreProcessor.Result result);
	}
}