using System;
using UnityEngine;

namespace ANovel.Engine
{
	public class MessageService : Service, ITextProcessor
	{
		public override Type ServiceType => typeof(MessageService);

		[SerializeField]
		TextPrinterBase m_TextPrinter;
		[SerializeField]
		MessageFrame m_Frame;
		public MessageFrame Frame => m_Frame;
		[SerializeField]
		FaceWindow m_FaceWindow;

		public bool IsProcessing => m_TextPrinter?.IsProcessing ?? false;

		protected override void Initialize()
		{
			Event.Register(this);
			m_TextPrinter?.Setup(Container);
			m_Frame?.Init();
			m_FaceWindow?.Init();
		}

		public void Set(TextBlock text, IEnvDataHolder data)
		{
			m_TextPrinter?.Set(text, data);
			if (data.TryGetSingle<MessageEnvData>(out var _))
			{
				m_FaceWindow.TryShow();
			}
		}

		protected override void OnUpdate(IEngineTime time)
		{
			m_Frame?.Update(time);
			m_TextPrinter?.OnUpdate(time);
		}

		public bool TryNext()
		{
			return m_TextPrinter?.TryNext() ?? true;
		}

		public void Clear()
		{
			m_TextPrinter?.Clear();
			ResetFace();
		}

		protected override void PreRestore(IMetaData meta, IEnvDataHolder data, IPreLoader loader)
		{
			m_TextPrinter?.PreRestore(meta, data, loader);
			if (data.TryGetSingle(out FaceWindowEnvData faceWindow) && !string.IsNullOrEmpty(faceWindow.Path))
			{
				var path = Path.CharaFaceWindowRoot + faceWindow.Path;
				loader.Load<Texture>(path);
			}
		}

		protected override void Restore(IMetaData meta, IEnvDataHolder data, IResourceCache cache)
		{
			ResetFace();
			m_TextPrinter?.Restore(meta, data, cache);
			m_Frame?.Restore(data);
			if (data.TryGetSingle(out FaceWindowEnvData faceWindow) && !string.IsNullOrEmpty(faceWindow.Path))
			{
				var config = faceWindow.CreateConfig();
				config.LoadTexture(Path.CharaFaceWindowRoot, cache);
				m_FaceWindow.ShowImmediate(config);
			}
		}

		[EventSubscribe(FaceWindowEvent.Show)]
		void FaceWindowShow(FaceWindowConfig args)
		{
			m_FaceWindow.Show(args);
		}

		[EventSubscribe(FaceWindowEvent.Update)]
		void FaceWindowUpdate(FaceWindowConfig args)
		{
			m_FaceWindow.Update(args);
		}

		[EventSubscribe(FaceWindowEvent.Hide)]
		void ResetFace()
		{
			m_FaceWindow.Reset();
		}


	}

}