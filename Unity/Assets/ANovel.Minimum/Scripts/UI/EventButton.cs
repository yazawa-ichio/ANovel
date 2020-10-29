using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Minimum
{
	public class EventButton : MonoBehaviour
	{
		[SerializeField]
		EngineEvent m_Event = EngineEvent.None;
		[SerializeField]
		bool m_AutoInteractable = true;

		Button m_Button;
		bool m_Interactable;

		void Awake()
		{
			m_Button = GetComponent<Button>();
			m_Button.onClick.AddListener(OnClick);
			if (m_AutoInteractable)
			{
				m_Button.interactable = m_Interactable = GetInteractable();
			}
		}

		void LateUpdate()
		{
			if (!m_AutoInteractable)
			{
				return;
			}
			var interactable = GetInteractable();
			if (m_Interactable != interactable)
			{
				m_Interactable = interactable;
				m_Button.interactable = m_Interactable;
			}
		}

		bool GetInteractable()
		{
			if (!ANMEngine.IsValid)
			{
				return false;
			}
			switch (m_Event)
			{
				case EngineEvent.AutoMode:
					return !ANMEngine.IsAutoMode;
				case EngineEvent.SkipMode:
					return !ANMEngine.IsSkipMode;
				case EngineEvent.Pause:
					return !ANMEngine.IsPause;
				case EngineEvent.Resume:
					return ANMEngine.IsPause;
			}
			return true;
		}

		void OnClick()
		{
			if (ANMEngine.IsValid)
			{
				ANMEngine.Dispatch(m_Event);
			}
		}

	}
}