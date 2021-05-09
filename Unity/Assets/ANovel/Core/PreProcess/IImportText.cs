namespace ANovel.Core
{
	public interface IImportText
	{
		bool Enabled { get; }
		string Path { get; }
		void Import(string text);
	}
}