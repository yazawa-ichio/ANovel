namespace ANovel
{
	public interface ICommand
	{
		bool IsPrepared();
		void Init(IServiceContainer container, IMetaData meta, IEnvData data);
		void Prepare(IPreLoader loader);
		bool IsSync();
		bool IsEnd();
		void Execute();
		void Update();
		void TryNext();
		void Finish();
	}

	public interface ISystemCommand : ICommand
	{
	}

}