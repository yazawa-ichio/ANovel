namespace ANovel.Core
{
	public interface IEnvDataCustomProcessor
	{
		int Priority { get; }
		void PreUpdate(EnvDataUpdateParam param);
		void PostUpdate(EnvDataUpdateParam param);
	}
}