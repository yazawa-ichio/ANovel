using System;
using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Engine
{

	public interface IScreenService
	{
		IScreenId CurrentId { get; }
		event Action<BeginSwapEvent> OnBeginSwap;
		event Action<DeleteChildEvent> OnDeleteChild;
		event Action<EndSwapEvent> OnEndSwap;
		ITransitionController Transition { get; }
		Vector2 Size { get; }
		ILevel GetLevel(string level);
	}

	public class BeginSwapEvent
	{
		public bool Copy;
		public IScreenId PrevId;
		public IScreenId NextId;
	}

	public class DeleteChildEvent
	{
		public IScreenId TargetId;
	}

	public class EndSwapEvent
	{
		public IScreenId CurrentId;
	}

	public class ScreenService : Service, IScreenService
	{
		[Serializable]
		class CustomLevel
		{
			public string Name = default;
			public int Order = default;
		}

		[SerializeField]
		RawImage m_Target = default;
		[SerializeField]
		Shader m_Shader = default;
		[SerializeField]
		bool m_BackgroundAlpha;
		[SerializeField]
		LayerMask m_LayerMask;
		[SerializeField]
		CustomLevel[] m_CustomLevel = Array.Empty<CustomLevel>();

		Transform m_Root;
		ScreenController m_ScreenController;
		ImagePool m_Pool;

		public override Type ServiceType => typeof(IScreenService);

		public override RegisterPriority Priority => RegisterPriority.High;

		public event Action<BeginSwapEvent> OnBeginSwap
		{
			add => m_ScreenController.OnBeginSwap += value;
			remove => m_ScreenController.OnBeginSwap -= value;
		}

		public event Action<DeleteChildEvent> OnDeleteChild
		{
			add => m_ScreenController.OnDeleteChild += value;
			remove => m_ScreenController.OnDeleteChild -= value;
		}

		public event Action<EndSwapEvent> OnEndSwap
		{
			add => m_ScreenController.OnEndSwap += value;
			remove => m_ScreenController.OnEndSwap -= value;
		}

		public ITransitionController Transition => m_ScreenController;

		public Vector2 Size => m_Target.rectTransform.rect.size;

		public IScreenId CurrentId => m_ScreenController.Current.ScreenId;

		protected override void Initialize()
		{
			if (m_Shader == null)
			{
				m_Shader = ShaderCache.Get(UIImageMaterial.ShaderName);
			}
			var obj = new GameObject(typeof(ScreenService).Name);
			obj.layer = gameObject.layer;
			m_Root = obj.transform;
			m_Root.SetParent(transform);
			m_ScreenController = new ScreenController(m_Root, m_Shader, m_BackgroundAlpha, m_LayerMask);
			m_ScreenController.SetTarget(m_Target);
			foreach (var level in Enum.GetValues(typeof(Level)))
			{
				m_ScreenController.AddLevel(level.ToString(), (int)level);
			}
			foreach (var level in m_CustomLevel)
			{
				m_ScreenController.AddLevel(level.Name, level.Order);
			}
			m_Pool = new ImagePool(m_Root, Container.Get<IEngineTime>());
			Container.Set(m_Pool);
		}

		protected override void OnUpdate(IEngineTime time)
		{
			m_ScreenController?.Update(time.DeltaTime);
		}

		void LateUpdate()
		{
			m_Pool?.TrySort();
			m_ScreenController?.LateUpdate();
		}

		void OnDestroy()
		{
			m_ScreenController?.Dispose();
			m_Pool?.Dispose();
		}

#if UNITY_EDITOR
		void OnValidate()
		{
			if (m_Shader == null)
			{
				m_Shader = ShaderCache.Get(UIImageMaterial.ShaderName);
			}
		}
#endif

		public ILevel GetLevel(string level)
		{
			return m_ScreenController.GetLevel(level);
		}

		protected override void PreRestore(IMetaData meta, IEnvDataHolder data, IPreLoader loader)
		{
			Transition.Prepare(new ScreenTransitionConfig
			{
				Copy = false,
				Time = Millisecond.FromSecond(0f)
			});
		}

		protected override void PostRestore(IMetaData meta, IEnvDataHolder data)
		{
			Transition.Start().Dispose();
		}

	}
}