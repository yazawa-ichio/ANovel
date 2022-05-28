using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ANovel.QuickStart
{
	public class QuickStart : MonoBehaviour
	{
		[SerializeField]
		string m_File = "QuickStart";

		ANovelEngine m_Engine;

		void Start()
		{
			m_Engine = FindObjectOfType<ANovelEngine>();
			Run();
		}

		async void Run()
		{
			m_Engine.Initialize();
			try
			{
				await m_Engine.Run(m_File);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}

		void Update()
		{
			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
			{
				return;
			}
			if (Input.GetMouseButtonDown(0))
			{
				m_Engine.TryNext();
			}
		}
	}
}