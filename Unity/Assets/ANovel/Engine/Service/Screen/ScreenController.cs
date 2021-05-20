using System;
using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Engine
{
	public interface ITransitionController
	{
		bool IsTransition { get; }
		void Prepare(ScreenTransitionConfig config);
		IPlayHandle Start();
	}

	public class ScreenController : ITransitionController, IDisposable
	{
		Transform m_Root;
		RawImage m_Target;
		ScreenView m_Current;
		ScreenView m_Prev;
		UIImageMaterial m_Material;
		Rect m_PrevRect;
		ScreenTransitionConfig m_TransitionConfig;
		RenderTexture m_Capture;
		FloatFadeHandle m_FadeHandle;

		public event Action<BeginSwapEvent> OnBeginSwap;

		public event Action<DeleteChildEvent> OnDeleteChild;

		public event Action<EndSwapEvent> OnEndSwap;

		public Shader ScreenShader
		{
			get => m_Material.RawMaterial.shader;
			set => m_Material.RawMaterial.shader = value;
		}

		public bool IsTransition => m_TransitionConfig != null;

		public ScreenView Current => m_Current;

		public ScreenController(Transform root, Shader shader, bool backgroundAlpha, LayerMask layerMask)
		{
			m_Root = root;
			m_Material = new UIImageMaterial(new Material(shader));
			m_Current = CreateView("Current");
			m_Current.IsCurrent = true;
			m_Current.Setup(backgroundAlpha, layerMask);
			m_Prev = CreateView("Prev");
			m_Prev.SetEnabled(false);
			m_Prev.Setup(backgroundAlpha, layerMask);
			m_Material.MainTex = m_Current.ViewTexture;
		}

		public void Dispose()
		{
			if (m_Capture != null)
			{
				RenderTexture.ReleaseTemporary(m_Capture);
				m_Capture = null;
			}
			m_Material?.Dispose();
			m_Material = null;
		}

		public void SetTarget(RawImage target)
		{
			m_Target = target;
			m_Target.material = m_Material.RawMaterial;
			m_PrevRect = Rect.zero;
			TryResize();
		}

		public void AddLevel(string name, int order)
		{
			m_Current.AddLevel(name, order);
			m_Prev.AddLevel(name, order);
		}

		public ILevel GetLevel(string level)
		{
			return m_Current.GetLevel(level);
		}

		ScreenView CreateView(string name)
		{
#if !UNITY_EDITOR
			name = "ScreenView";
#endif
			var obj = new GameObject(name);
			obj.transform.SetParent(m_Root);
			obj.layer = m_Root.gameObject.layer;
			return obj.AddComponent<ScreenView>();
		}

		public void Update(float deltaTime)
		{
			m_FadeHandle?.Update(deltaTime);
		}

		public void LateUpdate()
		{
			TryResize();
		}

		void TryResize()
		{
			var rect = m_Target.rectTransform.rect;
			if (rect != m_PrevRect)
			{
				m_PrevRect = rect;
				var screenSize = new Vector2Int(Mathf.FloorToInt(m_PrevRect.width), Mathf.FloorToInt(m_PrevRect.height));
				m_Current.SetRenderingSize(screenSize);
				m_Prev.SetRenderingSize(screenSize);
				if (IsTransition && !m_TransitionConfig.Capture)
				{
					m_Material.BackTex = m_Current.ViewTexture;
					m_Material.MainTex = m_Prev.ViewTexture;
				}
				else
				{
					m_Material.MainTex = m_Current.ViewTexture;
				}
				m_Target.SetMaterialDirty();
			}
		}

		void ITransitionController.Prepare(ScreenTransitionConfig config)
		{
			PrepareTransition(config);
		}

		public void PrepareTransition(ScreenTransitionConfig config)
		{
			m_FadeHandle?.Dispose();
			m_TransitionConfig = config;
			var id = m_Current.ScreenId;
			if (config.Capture)
			{
				var tex = m_Current.ViewTexture;
				m_Capture = RenderTexture.GetTemporary(tex.descriptor);
				Graphics.Blit(tex, m_Capture);
				m_Material.MainTex = m_Capture;
				m_Material.BackTex = tex;
			}
			else
			{
				var cur = m_Current;
				m_Current = m_Prev;
				m_Prev = cur;
				m_Current.SetEnabled(true);
				m_Current.IsCurrent = true;
				m_Prev.IsCurrent = false;
#if UNITY_EDITOR
				m_Current.name = "Current";
				m_Prev.name = "Prev";
#endif
				m_Material.MainTex = m_Prev.ViewTexture;
				m_Material.BackTex = m_Current.ViewTexture;
			}
			m_Material.RuleTex = config.Rule?.Value;
			m_Material.RuleVague = config.Vague / 2f;
			m_Material.TransValue = 0;
			m_Target.SetMaterialDirty();

			if (config.Capture && !config.Copy)
			{
				OnDeleteChild?.Invoke(new DeleteChildEvent
				{
					TargetId = m_Current.ScreenId
				});
				m_Current.ResetView();
			}
			else if (!config.Capture)
			{
				OnBeginSwap?.Invoke(new BeginSwapEvent
				{
					Copy = config.Copy,
					PrevId = id,
					NextId = m_Current.ScreenId,
				});
			}
		}

		IPlayHandle ITransitionController.Start()
		{
			return StartTransition();
		}

		public IPlayHandle StartTransition()
		{
			return m_FadeHandle = new FloatFadeHandle(1, 0, m_TransitionConfig.Time.ToSecond())
			{
				OnComplete = EndTransition,
				Output = (x) => m_Material.TransValue = x,
			};
		}

		void EndTransition()
		{
			m_FadeHandle = null;
			m_TransitionConfig = null;
			bool capture = m_Capture != null;
			m_Material.MainTex = m_Current.ViewTexture;
			if (capture)
			{
				RenderTexture.ReleaseTemporary(m_Capture);
				m_Capture = null;
			}
			else
			{
				m_Prev.SetEnabled(false);
			}
			m_Material.BackTex = null;
			m_Material.RuleTex = null;
			m_Material.TransValue = 0;
			m_Target.SetMaterialDirty();
			if (!capture)
			{
				OnDeleteChild?.Invoke(new DeleteChildEvent
				{
					TargetId = m_Prev.ScreenId
				});
				m_Prev.ResetView();
				OnEndSwap?.Invoke(new EndSwapEvent
				{
					CurrentId = m_Current.ScreenId,
				});
			}
		}

	}
}