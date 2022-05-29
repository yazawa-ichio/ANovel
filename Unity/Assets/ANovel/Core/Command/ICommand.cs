namespace ANovel
{
	public interface ICommand
	{
		void SetContainer(IServiceContainer container);
		void SetMetaData(IMetaData meta);
		void UpdateEnvData(IEnvData data);
		bool IsPrepared();
		void Initialize(IPreLoader loader);
		bool IsSync();
		bool IsEnd();
		void Execute();
		void Update();
		void TryNext();
		void FinishBlock();
	}

	public interface ISystemCommand : ICommand
	{
	}

}