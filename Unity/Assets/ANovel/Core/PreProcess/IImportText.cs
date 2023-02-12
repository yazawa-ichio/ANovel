namespace ANovel.Core
{
	public interface IImportText
	{
		string Path { get; }
		void Import(string text);
	}
}