#if ANOVEL_URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ANovel.Engine.PostEffects
{
	public class ANovelPostEffectPass : ScriptableRenderPass
	{
		RenderTargetIdentifier m_CameraColorTarget;
		RenderTargetHandle m_BufferHandle;

		public ANovelPostEffectPass()
		{
			renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
			m_BufferHandle.Init("ANovelPostEffectBuffer");
		}

		public void SetTarget(RenderTargetIdentifier cameraColorTarget)
		{
			m_CameraColorTarget = cameraColorTarget;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (!renderingData.cameraData.camera.TryGetComponent<PostEffectController>(out var controller))
			{
				return;
			}
			if (!controller.HasEffect)
			{
				return;
			}
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
		}

	}
}
#endif