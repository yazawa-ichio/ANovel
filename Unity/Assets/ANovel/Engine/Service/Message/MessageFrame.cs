using ANovel.Core;
using UnityEngine;

namespace ANovel.Service
{
	[System.Serializable]
	public class MessageFrame
	{
		[SerializeField]
		CanvasGroup m_Frame;
		[SerializeField]
		Vector3 m_HideOffset = new Vector3(0, -50, 0);
		[SerializeField]
		float m_ShowTime;
		[SerializeField]
		Easing m_ShowEasing;
		[SerializeField]
		float m_HideTime;
		[SerializeField]
		Easing m_HideEasing;

		FloatFadeHandle m_Handle;
		Vector3 m_ShowPos;
		Vector3 m_HidePos;

		public void Init()
		{
			if (m_Frame == null)
			{
				return;
			}
			m_ShowPos = m_Frame.transform.localPosition;
			m_HidePos = m_ShowPos + m_HideOffset;
		}

		public IPlayHandle Show()
		{
			if (m_Frame == null)
			{
				return FloatFadeHandle.Empty;
			}
			var value = m_Frame.alpha;
			m_Handle?.Dispose();
			return m_Handle = new FloatFadeHandle(1f, value, m_ShowTime * (1f - value))
			{
				Output = Set,
				Easing = m_ShowEasing.GetMethod(),
			};
		}

		public IPlayHandle Hide()
		{
			if (m_Frame == null)
			{
				return FloatFadeHandle.Empty;
			}
			var value = m_Frame.alpha;
			m_Handle?.Dispose();
			return m_Handle = new FloatFadeHandle(0f, value, m_HideTime * value)
			{
				Output = Set,
				Easing = m_HideEasing.GetMethod(),
			};
		}

		void Set(float value)
		{
			m_Frame.alpha = value;
			var pos = Vector3.Lerp(m_HidePos, m_ShowPos, value);
			m_Frame.transform.localPosition = pos;
		}

		public void Update(IEngineTime time)
		{
			m_Handle?.Update(time.DeltaTime);
		}

		public void Restore(IEnvDataHolder data)
		{
			if (m_Frame == null)
			{
				return;
			}
			m_Handle?.Dispose();
			data.TryGetSingle<MessageStatusEnvData>(out var status);
			Set(status.Hide ? 0 : 1f);
		}

	}
}