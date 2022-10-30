using ANovel.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ANovel.Engine
{
	public abstract class TextPrinterBase : MonoBehaviour
	{
		Queue<char> m_Buffer = new Queue<char>();
		float m_ShowTimer;
		string m_Lang;
		LocalizeTextEnvData[] m_LocalizeTexts;
		protected ServiceContainer Container { get; private set; }

		public bool IsProcessing { get; private set; }

		public virtual float Speed { get; set; } = 0.05f;

		protected virtual void SetChara(string name) { }

		protected abstract void SetName(string name);

		protected abstract void SetMessage(string text);

		protected abstract void AddMessage(string text);

		public void Setup(ServiceContainer container)
		{
			Container = container;
		}

		public virtual void Set(TextBlock text, IEnvDataHolder data, IMetaData meta)
		{
			m_LocalizeTexts = null;
			SetName("");
			SetMessage("");
			m_Buffer.Clear();
			m_ShowTimer = 0;
			IsProcessing = true;
			if (data.TryGetSingle<MessageEnvData>(out var message))
			{
				if (TryInitLocalizeText(meta, data))
				{
					message = GetLocalizedText(message);
				}
				SetChara(message.Chara);
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
			m_LocalizeTexts = null;
			SetChara("");
			SetName("");
			SetMessage("");
		}

		public void ChangeLanguage(string lang)
		{
			m_Lang = lang;
			if (m_LocalizeTexts != null)
			{
				m_Buffer.Clear();
				IsProcessing = false;
				var msg = GetLocalizedText(new MessageEnvData());
				SetName(msg.Name);
				SetMessage(msg.Message);
			}
		}

		public virtual void PreRestore(IMetaData meta, IEnvDataHolder data, IPreLoader loader)
		{
		}

		public virtual void Restore(IMetaData meta, IEnvDataHolder data, IResourceCache cache)
		{
			if (data.TryGetSingle<MessageEnvData>(out var msg))
			{
				if (TryInitLocalizeText(meta, data))
				{
					msg = GetLocalizedText(msg);
				}
				SetChara(msg.Chara);
				SetName(msg.Name);
				SetMessage(msg.Message);
			}
			else
			{
				m_LocalizeTexts = null;
				SetChara("");
				SetName("");
				SetMessage("");
			}
			m_Buffer.Clear();
			IsProcessing = false;
		}

		bool TryInitLocalizeText(IMetaData meta, IEnvDataHolder data)
		{
			if (meta.TryGetSingle<LocalizeMetaData>(out var localize))
			{
				m_LocalizeTexts = data.GetAll<LocalizeTextEnvData>().Select(x => x.Value).ToArray();
				return true;
			}
			return false;
		}

		protected bool TryGetLocalizedText(IMetaData meta, IEnvDataHolder data, out LocalizeTextEnvData current, out LocalizeTextEnvData[] texts)
		{
			current = default;
			texts = null;
			if (meta.TryGetSingle<LocalizeMetaData>(out var localize))
			{
				texts = data.GetAll<LocalizeTextEnvData>().Select(x => x.Value).ToArray();

				return true;
			}
			return false;
		}


		protected MessageEnvData GetLocalizedText(MessageEnvData message)
		{
			var ret = new MessageEnvData
			{
				Chara = message.Chara,
				Name = message.Name,
				Message = message.Message,
				RawText = message.RawText
			};
			LocalizeTextEnvData textEnvData;
			if (m_LocalizeTexts.Any(x => x.Lang == m_Lang))
			{
				textEnvData = m_LocalizeTexts.FirstOrDefault(x => x.Lang == m_Lang);
			}
			else
			{
				textEnvData = m_LocalizeTexts.First(x => x.Default);
			}
			var localizedText = textEnvData.CreateText();
			if (localizedText.TryParseName("【", "】", out _, out var dispName))
			{
				ret.Name = dispName;
				ret.Message = localizedText.GetRange(1);
			}
			else
			{
				ret.Message = localizedText.Get();
			}
			return ret;
		}

	}

}