namespace ANovel
{
	public interface IPlayingEnvDataProcessor
	{
		void Store(IEnvData data);
		void Restore(IEnvDataHolder data);
	}
}