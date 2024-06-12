#if ANOVEL_URP
using UnityEngine.Rendering.Universal;

namespace ANovel.Engine.PostEffects
{
	public class ANovelPostEffectFeature : ScriptableRendererFeature
	{
		ANovelPostEffectPass m_Pass;

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			m_Pass.SetTarget(renderer.cameraColorTarget);
			renderer.EnqueuePass(m_Pass);
		}

		public override void Create()
		{
			m_Pass = new ANovelPostEffectPass();
		}
	}
}
#endif