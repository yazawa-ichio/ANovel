using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ANovel.Engine
{

	public class ScreenView : MonoBehaviour
	{
		class LevelEntry : ILevel
		{
			public IScreenId ScreenId { get; set; }
			public string Name { get; set; }
			public int Order { get; set; }
			public RectTransform Root { get; set; }
		}

		Canvas m_Canvas;
		Camera m_Camera;
		bool m_BackgroundAlpha;
		LayerMask m_LayerMask;

		public Camera Camera => m_Camera;

		public RenderTexture ViewTexture { get; private set; }

		public RectTransform Root { get; private set; }

		public bool Enabled { get; private set; } = true;

		public bool IsCurrent { get; set; }

		public ScreenId ScreenId { get; private set; }

		Dictionary<string, LevelEntry> m_Levels = new Dictionary<string, LevelEntry>();

		void Awake()
		{
			ScreenId = new ScreenId(this);
		}

		public void Setup(bool backgroundAlpha, LayerMask layerMask)
		{
			m_BackgroundAlpha = backgroundAlpha;
			m_LayerMask = layerMask;
		}

		public void AddLevel(string name, int order)
		{
			if (!m_Levels.TryGetValue(name, out var level))
			{
				var obj = new GameObject(name);
				obj.transform.SetParent(Root);
				obj.layer = Root.gameObject.layer;
				var transform = obj.AddComponent<RectTransform>();
				ResetRootTransform(transform);
				m_Levels[name] = level = new LevelEntry
				{
					Name = name,
					Root = transform,
					ScreenId = ScreenId,
				};
			}
			level.Order = order;
			SortLevels();
		}

		static readonly string s_DefaultLevelName = Level.Center.ToString();
		public ILevel GetLevel(string level)
		{
			if (string.IsNullOrEmpty(level))
			{
				return m_Levels[s_DefaultLevelName];
			}
			return m_Levels[level];
		}

		void SortLevels()
		{
			foreach (var level in m_Levels.Values.OrderBy(x => x.Order))
			{
				level.Root.SetAsLastSibling();
			}
		}

		public void SetEnabled(bool enabled)
		{
			Enabled = enabled;
			if (m_Camera != null)
			{
				m_Camera.enabled = enabled;
			}
			if (m_Canvas != null)
			{
				m_Canvas.enabled = enabled;
			}
		}

		public void SetRenderingSize(Vector2Int size)
		{
			if (m_Camera == null)
			{
				var camera = new GameObject("Camera");
				camera.transform.SetParent(transform);
				camera.layer = gameObject.layer;
				m_Camera = camera.AddComponent<Camera>();
				m_Camera.clearFlags = CameraClearFlags.SolidColor;
				if (m_BackgroundAlpha)
				{
					m_Camera.backgroundColor = new Color();
				}
				else
				{
					m_Camera.backgroundColor = Color.black;
				}
				m_Camera.cullingMask = m_LayerMask;
				m_Camera.orthographic = true;
				m_Camera.nearClipPlane = -1000f;
				m_Camera.farClipPlane = 1000f;
				m_Camera.enabled = Enabled;
			}
			if (m_Canvas == null)
			{
				var canvas = new GameObject("Canvas");
				canvas.transform.SetParent(transform);
				canvas.layer = gameObject.layer;
				m_Canvas = canvas.AddComponent<Canvas>();
				m_Canvas.worldCamera = m_Camera;
				m_Canvas.renderMode = RenderMode.ScreenSpaceCamera;
				m_Canvas.enabled = Enabled;

				var root = new GameObject("Root");
				root.transform.SetParent(canvas.transform);
				Root = root.AddComponent<RectTransform>();
				ResetRootTransform(Root);
			}
			if (ViewTexture != null)
			{
				RenderTexture.ReleaseTemporary(ViewTexture);
			}
			var format = (m_BackgroundAlpha) ? RenderTextureFormat.ARGB32 : RenderTextureFormat.RGB565;
			if (!m_BackgroundAlpha && !SystemInfo.SupportsRenderTextureFormat(format))
			{
				format = RenderTextureFormat.ARGB32;
			}
			ViewTexture = RenderTexture.GetTemporary(size.x, size.y, 0, format);
			m_Camera.targetTexture = ViewTexture;
		}

		private void OnDestroy()
		{
			if (ViewTexture != null)
			{
				RenderTexture.ReleaseTemporary(ViewTexture);
				ViewTexture = null;
			}
		}

		void ResetRootTransform(RectTransform transform)
		{
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			transform.localPosition = Vector3.zero;
			transform.anchorMin = new Vector2(0, 0);
			transform.anchorMax = new Vector2(1, 1);
			transform.sizeDelta = new Vector2(0, 0);
		}

		public void ResetView()
		{
			ResetRootTransform(Root);
			foreach (var level in m_Levels.Values)
			{
				int count = level.Root.childCount;
				for (int i = 0; i < count; i++)
				{
					Destroy(level.Root.GetChild(i).gameObject);
				}
				ResetRootTransform(level.Root);
			}
		}

	}
}