using ANovel.Core;
using ANovel.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Service
{
	public struct MessageEnvData : IEnvDataValue<MessageEnvData>, IDefaultValueSerialization
	{
		public bool IsDefault => Equals(default);

		public string Name;
		public string Message;

		public bool Equals(MessageEnvData other)
		{
			return Name == other.Name && Message == other.Message;
		}
	}

	public class MessageService : Service, ITextProcessor
	{
		public static readonly string EnvKey = "MSG";

		public override Type ServiceType => typeof(MessageService);

		[SerializeField]
		Text m_NameText = default;
		[SerializeField]
		Text m_MessageText = default;
		[SerializeField]
		bool m_OneFrameOneCharLimit = true;

		Queue<char> m_Buffer = new Queue<char>();
		float m_ShowTimer;

		public bool IsProcessing { get; private set; }

		public void PreUpdate(TextBlock text, IEnvData data)
		{
			if (text == null)
			{
				data.Delete<MessageEnvData>(EnvKey);
				return;
			}
			var top = text.GetLine(0).Trim();
			if (top.StartsWith("【") && top.EndsWith("】"))
			{
				data.Set(EnvKey, new MessageEnvData
				{
					Name = top.Substring(1, top.Length - 2),
					Message = text.GetRange(1),
				});
			}
			else
			{
				data.Set(EnvKey, new MessageEnvData
				{
					Name = "",
					Message = text.Get(),
				});
			}
		}

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

		protected override void OnUpdate(IEngineTime time)
		{
			if (m_Buffer.Count > 0)
			{
				m_ShowTimer += time.DeltaTime;
			}
			else
			{
				IsProcessing = false;
			}
			while (m_Buffer.Count > 0 && m_ShowTimer > 0)
			{
				m_ShowTimer -= 0.05f;
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
				//m_NameText.text = "";
				//m_MessageText.text = "";
				IsProcessing = false;
			}
			return !run;
		}

		public void Clear()
		{
			m_NameText.text = "";
			m_MessageText.text = "";
		}

		protected override void Restore(IEnvDataHolder data, ResourceCache cache)
		{
			if (data.TryGet<MessageEnvData>(EnvKey, out var msg))
			{
				m_NameText.text = msg.Name;
				m_MessageText.text = msg.Message;
			}
			else
			{
				m_NameText.text = "";
				m_MessageText.text = "";
			}
			m_Buffer.Clear();
			IsProcessing = false;
		}

	}
}