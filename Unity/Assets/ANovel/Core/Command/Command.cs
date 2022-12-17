using ANovel.Core;

namespace ANovel
{
	public abstract class Command : Tag, ICommand
	{
		protected internal IServiceContainer Container { get; private set; }

		protected ScopedEventBroker Event { get; private set; }

		protected IResourceCache Cache { get; private set; }

		protected internal IMetaData Meta { get; private set; }

		void ICommand.Init(IServiceContainer container, IMetaData meta, IEnvData data)
		{
			Container = container;
			Meta = meta;
			UpdateEnvData(data);
		}

		protected virtual void UpdateEnvData(IEnvData data) { }

		void ICommand.Prepare(IPreLoader loader)
		{
			Event = Get<EventBroker>().Scoped();
			Event.Subscribe(this);
			Cache = Get<IResourceCache>();
			Prepare();
			Preload(loader);
		}

		protected virtual void Prepare() { }

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

		void ICommand.Finish()
		{
			try
			{
				Finish();
			}
			finally
			{
				Event.Dispose();
			}
		}

		public virtual void Finish() { }


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