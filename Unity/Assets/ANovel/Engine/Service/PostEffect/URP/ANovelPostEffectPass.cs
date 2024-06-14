#if ANOVEL_URP
using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ANovel.Engine.PostEffects
{
	public class ANovelPostEffectPass : ScriptableRenderPass, IDisposable
	{
		RenderTargetIdentifier m_CameraColorTarget;
#if ANOVEL_URP_14
		RTHandle m_BufferHandle;
#else
		RenderTargetHandle m_BufferHandle;
#endif

		public ANovelPostEffectPass()
		{
			renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
#if !ANOVEL_URP_14
			m_BufferHandle.Init("ANovelPostEffectBuffer");
#endif
		}

		public void SetTarget(RenderTargetIdentifier cameraColorTarget)
		{
			m_CameraColorTarget = cameraColorTarget;
		}

		public void Dispose()
		{
			ReleaseBuffer();
		}

		void ReleaseBuffer()
		{
#if ANOVEL_URP_14
			if (m_BufferHandle != null)
			{
				RTHandles.Release(m_BufferHandle);
				m_BufferHandle = null;
			}
#endif
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (!renderingData.cameraData.camera.TryGetComponent<PostEffectController>(out var controller))
			{
				ReleaseBuffer();
				return;
			}
			if (!controller.HasEffect)
			{
				ReleaseBuffer();
				return;
			}
#if ANOVEL_URP_14
			var targetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			{
				var cmd = CommandBufferPool.Get("Prepare PostEffect");
				cmd.Clear();
				RenderingUtils.ReAllocateIfNeeded(ref m_BufferHandle, targetDescriptor, name: "ANovelPostEffectBuffer");
				cmd.Blit(m_CameraColorTarget, m_BufferHandle);
				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
			foreach (var task in controller.GetTasks())
			{
				var cmd = CommandBufferPool.Get(task.ToString());
				task.Effect.SetCameraColor(m_CameraColorTarget);
				task.Effect.Execute(task.Param, cmd, targetDescriptor, m_BufferHandle);
				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
			{
				var cmd = CommandBufferPool.Get("Finish PostEffect");
				cmd.Clear();
				cmd.Blit(m_BufferHandle, m_CameraColorTarget);
				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
#else
			var targetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			{
				var cmd = CommandBufferPool.Get("Prepare PostEffect");
				cmd.Clear();
				cmd.GetTemporaryRT(m_BufferHandle.id, targetDescriptor);
				cmd.Blit(m_CameraColorTarget, m_BufferHandle.id);
				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
			foreach (var task in controller.GetTasks())
			{
				var cmd = CommandBufferPool.Get(task.ToString());
				task.Effect.SetCameraColor(m_CameraColorTarget);
				task.Effect.Execute(task.Param, cmd, targetDescriptor, m_BufferHandle.Identifier());
				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
			{
				var cmd = CommandBufferPool.Get("Finish PostEffect");
				cmd.Clear();
				cmd.Blit(m_BufferHandle.id, m_CameraColorTarget);
				cmd.ReleaseTemporaryRT(m_BufferHandle.id);
				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
#endif
		}

	}
}
#endif