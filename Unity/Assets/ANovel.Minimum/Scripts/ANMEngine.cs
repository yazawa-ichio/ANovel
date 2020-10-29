using ANovel.Core;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel.Minimum
{

	public class ANMEngine : MonoBehaviour
	{
		enum Mode
		{
			None,
			Skip,
			Auto,
		}

		static ANMEngine s_Instance;

		public static bool IsValid => s_Instance != null;

		public static bool IsRunning => s_Instance != null && s_Instance.m_Initialized && s_Instance.m_Conductor.IsRunning;

		public static bool IsSkipMode
		{
			get => s_Instance != null && s_Instance.m_Mode == Mode.Skip;
			set
			{
				if (value)
				{
					Dispatch(EngineEvent.SkipMode);
				}
				else if (IsSkipMode)
				{
					Dispatch(EngineEvent.None);
				}
			}
		}

		public static bool IsAutoMode
		{
			get => s_Instance != null && s_Instance.m_Mode == Mode.Auto;
			set
			{
				if (value)
				{
					Dispatch(EngineEvent.AutoMode);
				}
				else if (IsAutoMode)
				{
					Dispatch(EngineEvent.None);
				}
			}
		}

		public static bool IsPause
		{
			get => s_Instance != null && s_Instance.m_Pause;
		}

		public static void Initialize()
		{
			var setting = new Setting()
			{
				ResourceLoader = new ResourceLoader("ANovel"),
				FileLoader = new LocalFileLoader(Application.streamingAssetsPath + "/ANovel/Scenario"),
			};
			Initialize(setting);
		}

		public static void Initialize(Setting setting)
		{
			if (s_Instance.m_Initialized)
			{
				throw new InvalidOperationException("already initialized");
			}
			s_Instance.InitializeImpl(setting);
		}

		public static Task Run(string path, bool clear = true)
		{
			return s_Instance.RunImpl(path, clear);
		}

		public static void Next()
		{
			Dispatch(EngineEvent.Next);
		}

		public static void Dispatch(EngineEvent e)
		{
			s_Instance.DispatchImpl(e);
		}

		public static void Register<T>(T instance)
		{
			s_Instance.m_Conductor.Container.Set(instance);
		}

		[SerializeField]
		Config m_Config;

		Setting m_Setting;
		bool m_Initialized;
		Conductor m_Conductor;
		bool m_Pause;
		Mode m_Mode;
		float m_AutoTimer;

		public Config Config => m_Config;

		void Awake()
		{
			if (s_Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			s_Instance = this;
		}

		void OnDestroy()
		{
			m_Setting?.FileLoader?.Dispose();
			(m_Setting?.ResourceLoader as IDisposable)?.Dispose();
			if (s_Instance == this)
			{
				s_Instance = null;
			}
		}

		void InitializeImpl(Setting setting)
		{
			m_Initialized = true;
			m_Setting = setting;
			var reader = new BlockReader(setting.FileLoader, new string[] { "ANOVEL_MINIMUM" });
			m_Conductor = new Conductor(reader, setting.ResourceLoader, GetComponentInChildren<IMessageController>(true));
			if (m_Config == null)
			{
				m_Config = ScriptableObject.CreateInstance<Config>();
			}
			m_Conductor.Container.Set(m_Config);
			foreach (var controller in GetComponentsInChildren<IController>(true))
			{
				controller.Setup(m_Config);
				m_Conductor.Container.Set(controller.ControllerType, controller);
			}
		}

		private async Task RunImpl(string path, bool clear)
		{
			if (clear)
			{
				foreach (var controller in GetComponentsInChildren<IController>(true))
				{
					controller.Clear();
				}
			}
			await m_Conductor.Load(path);
			m_Pause = false;
			m_Conductor.TryNext();
		}

		void DispatchImpl(EngineEvent e)
		{
			switch (e)
			{
				case EngineEvent.Pause:
					m_Pause = true;
					foreach (var controller in GetComponentsInChildren<IController>(true))
					{
						controller.Pause();
					}
					break;
				case EngineEvent.Resume:
					m_Pause = false;
					foreach (var controller in GetComponentsInChildren<IController>(true))
					{
						controller.Resume();
					}
					break;
				case EngineEvent.AutoMode:
					m_Mode = Mode.Auto;
					TryNext();
					break;
				case EngineEvent.SkipMode:
					m_Mode = Mode.Skip;
					break;
				case EngineEvent.StopMode:
					m_Mode = Mode.None;
					break;
				case EngineEvent.Next:
					m_Mode = Mode.None;
					TryNext();
					break;
			}
		}

		void Update()
		{
			switch (m_Mode)
			{
				case Mode.Skip:
					TryNext();
					break;
				case Mode.Auto:
					ProcessAuto();
					break;
			}
			if (m_Initialized)
			{
				m_Conductor.Update();
			}
		}

		void TryNext()
		{
			if (!m_Pause)
			{
				m_AutoTimer = 0;
				m_Conductor.TryNext();
			}
		}
		void ProcessAuto()
		{
			if (m_Pause) return;
			if (!m_Conductor.IsProcessing)
			{
				m_AutoTimer += Time.deltaTime;
				if (m_AutoTimer > Config.AutoWait)
				{
					TryNext();
				}
			}
		}

	}
}