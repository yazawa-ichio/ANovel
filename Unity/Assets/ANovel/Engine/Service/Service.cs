using ANovel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel.Service
{
	public enum RegisterPriority
	{
		Low,
		Normal,
		High,
	}

	public interface IService
	{
		Type ServiceType { get; }
		RegisterPriority Priority { get; }
		void Initialize(ServiceContainer container);
		void OnUpdate(IEngineTime time);

		Task PreRestore(IEnvDataHolder data, IPreLoader loader);
		Task Restore(IEnvDataHolder data, ResourceCache cache);
		Task PostRestore(IEnvDataHolder data);
	}

	public abstract class Service : MonoBehaviour, IService
	{
		public abstract Type ServiceType { get; }

		public virtual RegisterPriority Priority => RegisterPriority.Normal;

		protected ServiceContainer Container { get; private set; }

		protected EngineConfig Config => Container.Get<EngineConfig>();

		protected PathConfig Path => Config.Path;

		protected virtual void Initialize() { }

		protected virtual void OnUpdate(IEngineTime time) { }

		protected virtual void PreRestore(IEnvDataHolder data, IPreLoader loader) { }
		protected virtual Task PreRestoreAsync(IEnvDataHolder data, IPreLoader loader) => Task.FromResult(true);
		protected virtual void Restore(IEnvDataHolder data, ResourceCache cache) { }
		protected virtual Task RestoreAync(IEnvDataHolder data, ResourceCache cache) => Task.FromResult(true);
		protected virtual void PostRestore(IEnvDataHolder data) { }
		protected virtual Task PostRestoreAsync(IEnvDataHolder data) => Task.FromResult(true);

		void IService.Initialize(ServiceContainer container)
		{
			Container = container;
			Initialize();
		}

		void IService.OnUpdate(IEngineTime time) => OnUpdate(time);

		Task IService.PreRestore(IEnvDataHolder data, IPreLoader loader)
		{
			PreRestore(data, loader);
			return PreRestoreAsync(data, loader);
		}

		Task IService.Restore(IEnvDataHolder data, ResourceCache cache)
		{
			Restore(data, cache);
			return RestoreAync(data, cache);
		}

		Task IService.PostRestore(IEnvDataHolder data)
		{
			PostRestore(data);
			return PostRestoreAsync(data);
		}

	}

}