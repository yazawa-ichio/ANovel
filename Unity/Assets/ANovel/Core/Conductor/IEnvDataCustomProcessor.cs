using ANovel.Core;

namespace ANovel
{
	public interface IEnvDataCustomProcessor
	{
		int Priority { get; }
		void PreUpdate(EnvDataUpdateParam param);
		void PostUpdate(EnvDataUpdateParam param);
		void PostJump(IMetaData meta, IEnvData data);
	}
}