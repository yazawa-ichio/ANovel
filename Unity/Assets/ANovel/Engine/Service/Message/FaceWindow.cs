using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Engine
{
	[System.Serializable]
	public class FaceWindow
	{
		[SerializeField]
		RawImage m_RawImage;
		[SerializeField]
		float m_ShowTime = 0.1f;
		[SerializeField]
		float m_HideTime = 0.1f;

		FaceWindowConfig m_FaceWindowConfig;
		bool m_ShowFlag;
		Coroutine m_HideCoroutine;

		public void Init()
		{
			if (m_RawImage != null)
			{
				m_RawImage.canvasRenderer.SetAlpha(0);
			}
		}

		public void TryShow()
		{
			if (m_RawImage != null && m_RawImage.texture != null && m_ShowFlag)
			{
				if (m_HideCoroutine != null)
				{
					m_RawImage.StopCoroutine(m_HideCoroutine);
					m_HideCoroutine = null;
				}
				m_RawImage.CrossFadeAlpha(1f, m_ShowTime, true);
			}
		}

		public void ShowImmediate(FaceWindowConfig args)
		{
			Show(args);
			if (m_RawImage != null && m_RawImage.texture != null)
			{
				if (m_HideCoroutine != null)
				{
					m_RawImage.StopCoroutine(m_HideCoroutine);
					m_HideCoroutine = null;
				}
				m_RawImage.canvasRenderer.SetAlpha(1f);
			}
		}

		public void Show(FaceWindowConfig args)
		{
			if (m_RawImage == null)
			{
				return;
			}
			m_ShowFlag = true;
			m_FaceWindowConfig?.Texture?.Dispose();
			m_FaceWindowConfig = args;
			if (args.Texture != null)
			{
				m_RawImage.enabled = true;
				m_RawImage.texture = args.Texture.Value;
				m_RawImage.SetNativeSize();
				m_RawImage.transform.localScale = Vector3.one * args.Scale.GetValueOrDefault(1f);
				m_RawImage.rectTransform.anchoredPosition3D = args.GetPos();
			}
			else
			{
				m_RawImage.texture = null;
				m_RawImage.enabled = false;
			}
		}

		public void Update(FaceWindowConfig args)
		{
			if (m_RawImage == null || args.Texture == null)
			{
				return;
			}
			m_RawImage.texture = args.Texture.Value;
			m_RawImage.SetNativeSize();
			m_RawImage.transform.localScale = Vector3.one * args.Scale.GetValueOrDefault(1f);
			m_RawImage.rectTransform.anchoredPosition3D = args.GetPos();
		}

		public void Reset()
		{
			m_ShowFlag = false;
			if (m_RawImage == null || m_HideCoroutine != null)
			{
				return;
			}
			m_HideCoroutine = m_RawImage.StartCoroutine(Hide());
		}

		IEnumerator Hide()
		{
			yield return null;
			m_RawImage.CrossFadeAlpha(0f, m_HideTime, true);
		}


	}

}