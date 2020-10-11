using ANovel.Core;

namespace ANovel
{
	public interface ICommand
	{
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