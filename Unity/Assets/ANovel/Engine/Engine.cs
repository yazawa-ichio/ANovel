using ANovel.Commands;
using ANovel.Core;
using ANovel.Service;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel
{

	public class Engine : MonoBehaviour
	{
		[SerializeField]
		EngineConfig m_Config;

		bool m_Initialized;
		CancellationTokenSource m_Cancellation = new CancellationTokenSource();

		Setting m_Setting;
		Conductor m_Conductor;
		IService[] m_Services;

		public EventBroker Event => m_Conductor.Event;

		public ServiceContainer Container => m_Conductor.Container;

		public event Action<Exception> OnError;

		public event Action OnStopCommand;

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
				ScenarioLoader = new LocalScenarioLoader(Application.streamingAssetsPath + "/ANovel/Scenario"),
				Time = new EngineTime(),
			};
			action?.Invoke(setting);
			Initialize(setting);
		}

		public void Initialize(Setting setting)
		{
			m_Initialized = true;
			m_Setting = setting;
			m_Conductor = setting.CreateConductor();
			m_Conductor.Text = GetComponentInChildren<ITextProcessor>(true);
			m_Conductor.OnError += HandleError;
			m_Conductor.OnLoad = Load;
			if (m_Config == null)
			{
				m_Config = ScriptableObject.CreateInstance<EngineConfig>();
			}
			m_Conductor.Container.Set(m_Config);
			m_Conductor.Event.Register(this);
			m_Services = GetComponentsInChildren<IService>(true).OrderBy(x => -(int)x.Priority).ToArray();
			foreach (var service in m_Services)
			{
				m_Conductor.Container.Set(service.ServiceType, service);
				service.Initialize(m_Conductor.Container);
			}
		}

		void HandleError(Exception ex)
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

		public Task Run(string path, string label = null)
		{
			return m_Conductor.Run(path, label, m_Cancellation.Token);
		}

		public Task Jump(string path, string label = null)
		{
			return m_Conductor.Jump(path, label, m_Cancellation.Token);
		}

		public Task Seek(string label)
		{
			return m_Conductor.Seek(label, m_Cancellation.Token);
		}

		public Task Seek(Func<Block, bool> match)
		{
			return m_Conductor.Seek(match, m_Cancellation.Token);
		}

		public bool TryNext()
		{
			return m_Conductor?.TryNext() ?? false;
		}

		public void Trigger(string name)
		{
			m_Conductor?.Event.Publish(EngineEvent.Trigger, name);
		}

		public Task Back()
		{
			return m_Conductor.Back(1, m_Cancellation.Token);
		}

		public Task Restore(StoreData data)
		{
			return m_Conductor.Restore(data, m_Cancellation.Token);
		}

		async Task Load(Block block, IEnvDataHolder data)
		{
			var restoreData = new RestoreData
			{
				Meta = block?.Meta,
				Env = data
			};
			var cache = m_Conductor.Container.Get<ResourceCache>();
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

		[EventSubscribe(ScenarioEvent.Stop)]
		void OnStopEvent()
		{
			OnStopCommand?.Invoke();
		}

	}
}