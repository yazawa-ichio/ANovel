using ANovel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Minimum
{
	public interface IMessageController : IController, ITextProcessor
	{
	}

	public class MessageController : ControllerBase, IMessageController
	{
		[SerializeField]
		Text m_NameText = default;
		[SerializeField]
		Text m_MessageText = default;
		[SerializeField]
		bool m_OneFrameOneCharLimit = true;

		Queue<char> m_Buffer = new Queue<char>();
		float m_ShowTimer;

		public override Type ControllerType => typeof(IMessageController);

		public bool IsProcessing { get; private set; }

		public void Set(TextBlock text)
		{
			m_NameText.text = "";
			m_MessageText.text = "";
			m_Buffer.Clear();

			var top = text.GetLine(0).Trim();
			if (top.StartsWith("【") && top.EndsWith("】"))
			{
				m_NameText.text = top.Substring(1, top.Length - 2);
				foreach (var c in text.GetRange(1))
				{
					m_Buffer.Enqueue(c);
				}
			}
			else
			{
				foreach (var c in text.Get())
				{
					m_Buffer.Enqueue(c);
				}
			}
			m_ShowTimer = 0;
			IsProcessing = true;
		}

		void Update()
		{
			if (m_Buffer.Count > 0)
			{
				m_ShowTimer += DeltaTime;
			}
			else
			{
				IsProcessing = false;
			}
			while (m_Buffer.Count > 0 && m_ShowTimer > 0)
			{
				m_ShowTimer -= Config.TextSpeed;
				m_MessageText.text += m_Buffer.Dequeue();
				if (m_OneFrameOneCharLimit)
				{
					break;
				}
			}
		}

		public bool TryNext()
		{
			bool run = m_Buffer.Count > 0;
			if (run)
			{
				var tmp = m_MessageText.text;
				while (m_Buffer.Count > 0)
				{
					tmp += m_Buffer.Dequeue();
				}
				m_MessageText.text = tmp;
				IsProcessing = false;
			}
			else
			{
				m_NameText.text = "";
				m_MessageText.text = "";
				IsProcessing = false;
			}
			return !run;
		}

	}
}