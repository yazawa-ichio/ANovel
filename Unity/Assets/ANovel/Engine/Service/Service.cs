//using ANovel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel.Engine
{
	public enum RegisterPriority
	{
		Low,
		Normal,
		High,
	}

	public class RestoreData
	{
		public IMetaData Meta;
		public IEnvDataHolder Env;
	}

	public interface IService
	{
		Type ServiceType { get; }
		RegisterPriority Priority { get; }
		void Initialize(ServiceContainer container, string language);
		void OnUpdate(IEngineTime time);

		Task PreRestore(RestoreData data, IPreLoader loader);
		Task Restore(RestoreData data, IResourceCache cache);
		Task PostRestore(RestoreData data);
		void ChangeLanguage(string language);
	}

	public abstract class Service : MonoBehaviour, IService
	{
		public abstract Type ServiceType { get; }

		public virtual RegisterPriority Priority => RegisterPriority.Normal;

		protected ServiceContainer Container { get; private set; }

		protected EngineConfig Config => Container.Get<EngineConfig>();

		protected PathConfig Path => Config.Path;

		protected EventBroker Event => Container.Get<EventBroker>();

		protected virtual void Initialize() { }

		protected virtual void OnUpdate(IEngineTime time) { }

		protected virtual void PreRestore(IMetaData meta, IEnvDataHolder data, IPreLoader loader) { }
		protected virtual Task PreRestoreAsync(IMetaData meta, IEnvDataHolder data, IPreLoader loader) => Task.FromResult(true);
		protected virtual void Restore(IMetaData meta, IEnvDataHolder data, IResourceCache cache) { }
		protected virtual Task RestoreAync(IMetaData meta, IEnvDataHolder data, IResourceCache cache) => Task.FromResult(true);
		protected virtual void PostRestore(IMetaData meta, IEnvDataHolder data) { }
		protected virtual Task PostRestoreAsync(IMetaData meta, IEnvDataHolder data) => Task.FromResult(true);
		public virtual void ChangeLanguage(string language) { }

		void IService.Initialize(ServiceContainer container, string language)
		{
			Container = container;
			Event.Register(this);
			Initialize();
			if (!string.IsNullOrEmpty(language))
			{
				ChangeLanguage(language);
			}
		}

		void IService.OnUpdate(IEngineTime time) => OnUpdate(time);

		Task IService.PreRestore(RestoreData data, IPreLoader loader)
		{
			PreRestore(data.Meta, data.Env, loader);
			return PreRestoreAsync(data.Meta, data.Env, loader);
		}

		Task IService.Restore(RestoreData data, IResourceCache cache)
		{
			Restore(data.Meta, data.Env, cache);
			return RestoreAync(data.Meta, data.Env, cache);
		}

		Task IService.PostRestore(RestoreData data)
		{
			PostRestore(data.Meta, data.Env);
			return PostRestoreAsync(data.Meta, data.Env);
		}

	}

}