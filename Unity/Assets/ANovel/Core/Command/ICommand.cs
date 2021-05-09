using ANovel.Core;

namespace ANovel
{
	public interface ICommand
	{
		void SetMetaData(IMetaData meta);
		void UpdateEnvData(IEnvData data);
		bool IsPrepared();
		void Initialize(IServiceContainer container);
		bool IsSync();
		bool IsEnd();
		void Execute();
		void Update();
		void TryNext();
		void FinishBlock();
	}
}