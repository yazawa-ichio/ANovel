using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace ANovel.Engine.PostEffects
{
	public interface IPostEffectCollection
	{
		void Add(PostEffectTask task);
		void Remove(PostEffectTask task);
		void Clear();
	}

	[AddComponentMenu("")]
	public class PostEffectController : MonoBehaviour, IPostEffectCollection
	{
		Camera m_Camera;
		CommandBuffer m_CommandBuffer;
		int m_BufferHandle;

		List<PostEffectTask> m_Task = new();

		public bool HasEffect => GetTasks().Any();

		public IScreenId ScreenId { get; private set; }

		internal void Setup(IScreenId screenId)
		{
			ScreenId = screenId;
		}

		internal IEnumerable<PostEffectTask> GetTasks()
		{
			return m_Task.Where(x => x.Effect.Enabled).OrderBy(x => x.Priority);
		}

		public void Add(PostEffectTask task)
		{
			m_Task.Add(task);
		}

		public void Remove(PostEffectTask task)
		{
			m_Task.Remove(task);
		}

		public void Clear()
		{
			m_Task.Clear();
		}

		void Awake()
		{
			m_Camera = GetComponent<Camera>();
			m_BufferHandle = Shader.PropertyToID("ANovelPostEffectBuffer");
			m_CommandBuffer = new CommandBuffer();
			m_CommandBuffer.name = "ANovelPostEffect";
			m_Camera.AddCommandBuffer(CameraEvent.AfterImageEffects, m_CommandBuffer);
		}

		void OnPreRender()
		{
			m_CommandBuffer.Clear();
			if (!HasEffect)
			{
				return;
			}
			var cmd = m_CommandBuffer;
			var targetTexture = m_Camera.targetTexture;
			var targetDescriptor = new RenderTextureDescriptor(targetTexture.width, targetTexture.height, targetTexture.format);
			var target = new RenderTargetIdentifier(m_Camera.targetTexture);
			cmd.GetTemporaryRT(m_BufferHandle, targetDescriptor);
			cmd.Blit(target, m_BufferHandle);
			foreach (var effect in GetTasks())
			{
				effect.Effect.SetCameraColor(target);
				effect.Effect.Execute(effect.Param, cmd, targetDescriptor, m_BufferHandle);
			}
			cmd.Blit(m_BufferHandle, target);
			cmd.ReleaseTemporaryRT(m_BufferHandle);
		}

	}
}