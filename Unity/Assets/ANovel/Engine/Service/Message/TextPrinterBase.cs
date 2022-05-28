using System.Collections.Generic;
using UnityEngine;

namespace ANovel.Engine
{
	public abstract class TextPrinterBase : MonoBehaviour
	{
		Queue<char> m_Buffer = new Queue<char>();
		float m_ShowTimer;
		protected ServiceContainer Container { get; private set; }

		public bool IsProcessing { get; private set; }

		public virtual float Speed { get; set; } = 0.05f;

		protected abstract void SetName(string name);

		protected abstract void SetMessage(string text);

		protected abstract void AddMessage(string text);

		public void Setup(ServiceContainer container)
		{
			Container = container;
		}

		public virtual void Set(TextBlock text, IEnvDataHolder data)
		{
			SetName("");
			SetMessage("");
			m_Buffer.Clear();
			m_ShowTimer = 0;
			IsProcessing = true;
			if (data.TryGetSingle<MessageEnvData>(out var message))
			{
				SetName(message.Name);
				foreach (var c in message.Message)
				{
					m_Buffer.Enqueue(c);
				}
			}
		}

		public virtual void OnUpdate(IEngineTime time)
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
				m_ShowTimer -= Speed;
				AddMessage(m_Buffer.Dequeue().ToString());
			}
		}

		public bool TryNext()
		{
			bool run = m_Buffer.Count > 0;
			if (run)
			{
				var tmp = "";
				while (m_Buffer.Count > 0)
				{
					tmp += m_Buffer.Dequeue();
				}
				AddMessage(tmp);
				IsProcessing = false;
			}
			else
			{
				IsProcessing = false;
			}
			return !run;
		}

		public virtual void Clear()
		{
			SetName("");
			SetMessage("");
		}

		public virtual void PreRestore(IMetaData meta, IEnvDataHolder data, IPreLoader loader)
		{
		}

		public virtual void Restore(IMetaData meta, IEnvDataHolder data, IResourceCache cache)
		{
			if (data.TryGetSingle<MessageEnvData>(out var msg))
			{
				SetName(msg.Name);
				SetMessage(msg.Message);
			}
			else
			{
				SetName("");
				SetMessage("");
			}
			m_Buffer.Clear();
			IsProcessing = false;
		}

	}

}