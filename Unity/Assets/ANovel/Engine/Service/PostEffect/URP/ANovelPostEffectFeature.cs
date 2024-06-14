#if ANOVEL_URP
using UnityEngine.Rendering.Universal;

namespace ANovel.Engine.PostEffects
{
	public class ANovelPostEffectFeature : ScriptableRendererFeature
	{
		ANovelPostEffectPass m_Pass;

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
#if ANOVEL_URP_14
			m_Pass.SetTarget(renderer.cameraColorTargetHandle);
#else
			m_Pass.SetTarget(renderer.cameraColorTarget);
#endif
			renderer.EnqueuePass(m_Pass);
		}

		public override void Create()
		{
			m_Pass?.Dispose();
			m_Pass = new ANovelPostEffectPass();
		}

		protected override void Dispose(bool disposing)
		{
			m_Pass?.Dispose();
		}
	}
}
#endif