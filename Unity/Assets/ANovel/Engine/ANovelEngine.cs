using ANovel.Commands;
using ANovel.Core;
using ANovel.Engine;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel
{

	public class ANovelEngine : MonoBehaviour
	{
		[SerializeField]
		EngineConfig m_Config;

		bool m_Initialized;
		CancellationTokenSource m_Cancellation = new CancellationTokenSource();

		Setting m_Setting;
		Conductor m_Conductor;
		IService[] m_Services;
		ScopeLocker m_Locker = new ScopeLocker();

		public EngineConfig Config => m_Config;

		public EventBroker Event => m_Conductor.Event;

		public ServiceContainer Container => m_Conductor.Container;

		public IHistory History => m_Conductor.History;

		public event Action<Exception> OnError;

		public event Action OnStopCommand;

		public bool IsWaitNext => m_Conductor.IsWaitNext;

		public bool IsBusy => m_Locker.IsLock || !IsWaitNext;

		void OnDestroy()
		{
			m_Setting?.ScenarioLoader?.Dispose();
			(m_Setting?.ResourceLoader as IDisposable)?.Dispose();
			m_Cancellation?.Cancel();
			m_Cancellation = null;
			m_Conductor?.Dispose();
		}

		void Update()
		{
			if (m_Initialized)
			{
				foreach (var service in m_Services)
				{
					service.OnUpdate(m_Setting.Time);
				}
				m_Conductor.Update();
			}
		}

		public void Initialize(Action<Setting> action = null)
		{
			var setting = new Setting()
			{
				ResourceLoader = new ResourceLoader("ANovel"),
				ScenarioLoader = new ResourcesScenarioLoader("ANovel/Scenario"),
				Time = new EngineTime(),
			};
			action?.Invoke(setting);
			Initialize(setting);
		}

		public void Initialize(Setting setting)
		{
			m_Initialized = true;
			if (m_Config == null)
			{
				m_Config = ScriptableObject.CreateInstance<EngineConfig>();
			}
			m_Setting = setting;
			m_Conductor = setting.CreateConductor(m_Config);
			m_Conductor.Text = GetComponentInChildren<ITextProcessor>(true);
			m_Conductor.OnError += HandleError;
			m_Conductor.OnLoad = Load;
			m_Conductor.Container.Set(m_Config);
			m_Conductor.Event.Register(this);
			m_Services = GetComponentsInChildren<IService>(true).OrderBy(x => -(int)x.Priority).ToArray();
			foreach (var service in m_Services)
			{
				m_Conductor.Container.Set(service.ServiceType, service);
				service.Initialize(m_Conductor.Container);
			}
		}

		public void HandleError(Exception ex)
		{
			if (OnError == null)
			{
				Debug.LogException(ex);
			}
			else
			{
				OnError.Invoke(ex);
			}
		}

		public async void Handle(Func<Task> task)
		{
			try
			{
				await task();
			}
			catch (Exception ex)
			{
				HandleError(ex);
			}
		}

		public async Task Run(string path, string label = null)
		{
			using (m_Locker.ExclusiveLock())
			{
				await m_Conductor.Run(path, label, m_Cancellation.Token);
			}
		}

		public async Task Jump(string path, string label = null)
		{
			using (m_Locker.ExclusiveLock())
			{
				await m_Conductor.Jump(path, label, m_Cancellation.Token);
			}
		}

		public async Task Seek(string label)
		{
			using (m_Locker.ExclusiveLock())
			{
				await m_Conductor.Seek(label, m_Cancellation.Token);
			}
		}

		public async Task Seek(Func<Block, bool> match)
		{
			using (m_Locker.ExclusiveLock())
			{
				await m_Conductor.Seek(match, m_Cancellation.Token);
			}
		}

		public bool TryNext()
		{
			if (m_Locker.IsLock)
			{
				return false;
			}
			return m_Conductor?.TryNext() ?? false;
		}

		public bool Trigger(string name)
		{
			if (m_Locker.IsLock)
			{
				return false;
			}
			m_Conductor?.Event.Publish(EngineEvent.Trigger, name);
			return true;
		}

		public async Task Back()
		{
			using (m_Locker.ExclusiveLock())
			{
				await m_Conductor.Back(1, m_Cancellation.Token);
			}
		}

		public async Task Back(IHistoryLog log)
		{
			using (m_Locker.ExclusiveLock())
			{
				await m_Conductor.Back(log, m_Cancellation.Token);
			}
		}

		public void ReplayVoice(IHistoryLog log)
		{
			if (Container.TryGet(out IReplayVoiceService voice))
			{
				Handle(async () =>
				{
					using (Lock())
					{
						await voice.ReplayVoice(log);
					}
				});
			}
		}

		public async Task Restore(StoreData data)
		{
			using (m_Locker.ExclusiveLock())
			{
				await m_Conductor.Restore(data, m_Cancellation.Token);
			}
		}

		async Task Load(Block block, IEnvDataHolder data)
		{
			using (m_Locker.Lock())
			{
				var restoreData = new RestoreData
				{
					Meta = block?.Meta,
					Env = data
				};
				var cache = m_Conductor.Cache;
				using (var loader = new PreLoadScope(cache))
				{
					await Task.WhenAll(m_Services.Select(x => x.PreRestore(restoreData, loader)));

					if (!loader.IsLoaded)
					{
						await loader.WaitComplete();
					}

					await Task.WhenAll(m_Services.Select(x => x.Restore(restoreData, cache)));

					await Task.WhenAll(m_Services.Select(x => x.PostRestore(restoreData)));

				}
			}
		}

		public IDisposable Lock() => m_Locker.Lock();

		[EventSubscribe(ScenarioEvent.Stop)]
		void OnStopEvent()
		{
			OnStopCommand?.Invoke();
		}

	}
}