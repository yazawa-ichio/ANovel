using ANovel.Core;

namespace ANovel
{
	public abstract class Command : Tag, ICommand
	{
		protected IServiceContainer Container { get; private set; }

		protected ScopedEventBroker Event { get; private set; }

		protected IResourceCache Cache { get; private set; }

		protected IMetaData Meta { get; private set; }

		void ICommand.SetContainer(IServiceContainer container)
		{
			Container = container;
		}

		void ICommand.SetMetaData(IMetaData meta)
		{
			Meta = meta;
		}

		void ICommand.UpdateEnvData(IEnvData data) => UpdateEnvData(data);

		protected virtual void UpdateEnvData(IEnvData data) { }

		void ICommand.Initialize(IPreLoader loader)
		{
			Event = Get<EventBroker>().Scoped();
			Event.Subscribe(this);
			Cache = Get<IResourceCache>();
			Initialize();
			Preload(loader);
		}

		protected virtual void Initialize() { }

		protected virtual void Preload(IPreLoader loader) { }

		public virtual bool IsPrepared() => true;

		public virtual bool IsSync() => true;

		public virtual bool IsEnd() => true;

		void ICommand.Execute() => Execute();

		protected virtual void Execute() { }

		void ICommand.Update() => Update();

		protected virtual void Update() { }

		void ICommand.TryNext() => TryNext();

		protected virtual void TryNext() { }

		void ICommand.FinishBlock()
		{
			try
			{
				FinishBlock();
			}
			finally
			{
				Event.Dispose();
			}
		}

		public virtual void FinishBlock() { }


		protected T Get<T>()
		{
			return Container.Get<T>();
		}

		protected bool TryGet<T>(out T value)
		{
			return Container.TryGet<T>(out value);
		}

	}


}