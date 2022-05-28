using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Engine
{


	public class ImageObject : MonoBehaviour
	{
		struct ParamPlayHandle
		{
			public ImageParamType ParamType;
			public FloatFadeHandle Handle;
		}

		ImagePool m_Owner;
		RawImage m_Image;
		UIImageMaterial m_Material;
		RectTransform m_Transform;
		ICacheHandle<Texture> m_MainTexHandle;
		ICacheHandle<Texture> m_BackTexHandle;
		ICacheHandle<Texture> m_RuleTexHandle;
		FloatFadeHandle m_TransitionHandle;
		List<ParamPlayHandle> m_Playing = new List<ParamPlayHandle>();
		IEngineTime m_Time;

		public ILevel Level { get; private set; }

		public RectTransform Transform => m_Transform;

		public bool IsTransition => m_TransitionHandle?.IsPlaying ?? false;

		public bool Visible
		{
			get => m_Image.enabled;
			set => m_Image.enabled = true;
		}

		public Vector2? TexSize { get; private set; }

		ImageLayout m_Layout = new ImageLayout();
		bool m_LayoutDirty = false;

		public long AutoOrder { get; private set; }

		void Awake()
		{
			m_Image = gameObject.AddComponent<RawImage>();
			m_Image.raycastTarget = false;
			m_Transform = m_Image.rectTransform;
			m_Material = new UIImageMaterial();
			m_Material.MainTex = null;
			m_Image.material = m_Material.RawMaterial;
			ResetParam();
		}

		void Update()
		{
			Update(m_Time?.DeltaTime ?? Time.deltaTime);
		}

		void LateUpdate()
		{
			ApplyLayout();
		}

		public void SetOwner(ImagePool owner)
		{
			m_Owner = owner;
		}

		public void SetTime(IEngineTime time)
		{
			m_Time = time;
		}

		public void SetLevel(ILevel level)
		{
			Level = level;
			m_Transform.SetParent(level.Root, true);
			m_Transform.localScale = Vector3.one;
		}

		public void SetConfig(ImageObjectConfig config)
		{
			SetOrder(config.AutoOrder);
			var texture = config.Texture;
			m_TransitionHandle?.Dispose();
			m_MainTexHandle?.Dispose();
			m_MainTexHandle = texture;
			m_Material.MainTex = texture.Value;
			m_Image.SetMaterialDirty();
			if (texture != null && texture.Value != null)
			{
				TexSize = new Vector2(texture.Value.width, texture.Value.height);
			}
			else
			{
				TexSize = null;
			}
		}

		public FloatFadeHandle Transition(ImageObjectConfig config)
		{
			SetOrder(config.AutoOrder);
			m_TransitionHandle?.Dispose();
			m_Material.BackTex = config.Texture?.Value;
			m_Material.RuleTex = config.RuleTexture?.Value;
			m_Material.RuleVague = config.Vague / 2f;
			m_Material.TransValue = 0;
			m_Image.SetMaterialDirty();
			if (config.Texture != null && config.Texture.Value != null)
			{
				TexSize = new Vector2(config.Texture.Value.width, config.Texture.Value.height);
			}
			else
			{
				TexSize = null;
			}
			if (config.Time.Value <= 0)
			{
				EndTransition();
				return FloatFadeHandle.Empty;
			}
			return m_TransitionHandle = new FloatFadeHandle(1, 0, config.Time.ToSecond())
			{
				OnComplete = EndTransition,
				Output = (x) => m_Material.TransValue = x,
			};
		}

		void EndTransition()
		{
			m_TransitionHandle = null;
			m_Material.BackToMain();
			m_Material.TransValue = 0;
			m_MainTexHandle?.Dispose();
			m_MainTexHandle = m_BackTexHandle;
			m_BackTexHandle = null;
			m_RuleTexHandle?.Dispose();
			m_RuleTexHandle = null;
			m_Image.SetMaterialDirty();
		}

		public ImageLayout GetLayout(bool current = true)
		{
			if (current)
			{
				return m_Layout;
			}
			var layout = m_Layout;
			foreach (var playing in m_Playing)
			{
				layout.Set(playing.ParamType, playing.Handle.To);
			}
			return layout;
		}

		public void SetOrder(long? order)
		{
			if (order.HasValue)
			{
				AutoOrder = order.Value;
				m_Owner.SetOrderDitry();
			}
		}

		public void SetLayout(ImageLayout layout)
		{
			StopAll();
			m_Layout = layout;
			m_LayoutDirty = true;
			ApplyLayout();
		}

		void ApplyLayout()
		{
			if (!m_LayoutDirty) return;
			m_LayoutDirty = false;
			m_Transform.localPosition = m_Layout.Pos;
			m_Transform.sizeDelta = m_Layout.Size;
			m_Transform.localEulerAngles = m_Layout.Angle;
			var col = m_Layout.Color;
			col.a = m_Layout.Opacity;
			if (IsTransition)
			{
				m_Material.BackColor = col;
			}
			else
			{
				m_Material.Color = col;
			}
		}

		void StopAll()
		{
			m_LayoutDirty = true;
			while (m_Playing.Count > 0)
			{
				m_Playing[0].Handle?.Dispose();
				m_Playing.RemoveAt(0);
			}
		}

		public void Stop(ImageParamType type)
		{
			m_LayoutDirty = true;
			for (int i = 0; i < m_Playing.Count; i++)
			{
				if (m_Playing[i].ParamType == type)
				{
					m_Playing[i].Handle?.Dispose();
					m_Playing.RemoveAt(i);
					break;
				}
			}
		}

		public IPlayHandle PlayAnim(ImageObjectParamAnimConfig[] configs)
		{
			return new CombinePlayHandle(configs.Select(x => PlayAnim(x)));
		}

		public IPlayHandle PlayAnim(ImageObjectParamAnimConfig config)
		{
			var type = config.Type;
			Stop(type);
			var from = m_Layout.Get(type);
			var handle = new FloatFadeHandle(config.Value, from, config.Time.ToSecond())
			{
				Easing = config.Easing.GetMethodOrNull(),
				Output = (x) => SetParamImpl(type, x),
			};
			m_Playing.Add(new ParamPlayHandle
			{
				ParamType = type,
				Handle = handle,
			});
			return handle;
		}

		public void SetParam(ImageParamType type, float value)
		{
			Stop(type);
			SetParamImpl(type, value);
		}

		void SetParamImpl(ImageParamType type, float value)
		{
			m_LayoutDirty = true;
			m_Layout.Set(type, value);
		}

		public void Update(float deltaTime)
		{
			for (int i = 0; i < m_Playing.Count; i++)
			{
				m_Playing[i].Handle.Update(deltaTime);
				if (!m_Playing[i].Handle.IsPlaying)
				{
					m_Playing.RemoveAt(i);
				}
			}
			m_TransitionHandle?.Update(deltaTime);
		}

		void ResetParam()
		{
			m_TransitionHandle?.Dispose();
			foreach (var playing in m_Playing)
			{
				playing.Handle.Dispose();
			}
			m_Playing.Clear();

			m_Material.Reset();
			m_MainTexHandle?.Dispose();
			m_MainTexHandle = null;
			m_BackTexHandle?.Dispose();
			m_BackTexHandle = null;
			m_RuleTexHandle?.Dispose();
			m_RuleTexHandle = null;

			m_Transform.sizeDelta = new Vector2(16, 16);
			m_Transform.localPosition = Vector3.zero;
			m_Transform.localRotation = Quaternion.identity;
			m_Transform.localScale = Vector3.one;

			m_Layout.Pos = m_Transform.localPosition;
			m_Layout.Size = m_Transform.sizeDelta;
			m_Layout.Angle = m_Transform.localEulerAngles;
			m_Layout.Color = Color.white;
			m_Layout.Opacity = 1f;
			m_LayoutDirty = false;
		}

		public void OnReturn()
		{
			Level = null;
			ResetParam();
		}

		public void Copy(ImageObject src, bool copyPlaying)
		{
			src.m_TransitionHandle?.Dispose();
			m_TransitionHandle?.Dispose();
			StopAll();
			m_MainTexHandle = src.m_MainTexHandle?.Duplicate();
			m_Material.Copy(src.m_Material);
			SetLayout(src.GetLayout());
			if (copyPlaying)
			{
				foreach (var playing in src.m_Playing)
				{
					var type = playing.ParamType;
					var handle = new FloatFadeHandle(playing.Handle.To, playing.Handle.From, playing.Handle.Time)
					{
						Easing = playing.Handle.Easing,
						Output = (x) => SetParamImpl(type, x),
					};
					handle.Update(playing.Handle.Timer);
					m_Playing.Add(new ParamPlayHandle
					{
						ParamType = type,
						Handle = handle,
					});
				}
			}
		}


	}
}