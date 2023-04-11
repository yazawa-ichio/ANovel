using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ANovel.Engine.PostEffects
{
	public struct BloomParam : IPostEffectParam
	{
		public static readonly BloomParam Default = new BloomParam()
		{
			Intensity = 0.8f,
			Threshold = 0.5f,
			SoftKnee = 0.2f,
		};

		[Range(0f, 10f), RateArgument]
		public float Intensity;
		[Range(0f, 1f), RateArgument]
		public float Threshold;
		[Range(0f, 1f), RateArgument]
		public float SoftKnee;
	}

	[Serializable]
	public class Bloom : PostEffectBase<BloomParam>
	{

		Material m_Material;
		RenderTargetHandle m_TempHandle1;
		RenderTargetHandle m_TempHandle2;
		int m_IntensityId;

		[SerializeField]
		protected Shader m_Shader;

		protected override void OnValidate()
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("ANovel/Bloom");
			}
		}

		protected override void Execute(BloomParam param, CommandBuffer cmd, RenderTextureDescriptor targetDescriptor, RenderTargetIdentifier target)
		{
			if (m_Material == null)
			{
				m_Material = new Material(m_Shader);
				m_TempHandle1.Init("_TempRT1");
				m_TempHandle2.Init("_TempRT2");
				m_IntensityId = Shader.PropertyToID("_Intensity");
			}
			m_Material.SetFloat(m_IntensityId, param.Intensity);
			m_Material.SetFloat("_Threshold", param.Threshold);
			m_Material.SetFloat("_SoftKnee", param.SoftKnee);

			var descriptor = targetDescriptor;
			descriptor.depthBufferBits = 0;
			descriptor.colorFormat = RenderTextureFormat.R8;

			// Prefilter
			descriptor.width = (int)(descriptor.width / 2f);
			descriptor.height = (int)(descriptor.height / 2f);
			cmd.GetTemporaryRT(m_TempHandle1.id, descriptor);
			cmd.Blit(target, m_TempHandle1.Identifier(), m_Material, 0);

			RenderTargetIdentifier dst;

			// Downsample
			descriptor.width = (int)(descriptor.width / 2f);
			descriptor.height = (int)(descriptor.height / 2f);
			cmd.GetTemporaryRT(m_TempHandle2.id, descriptor);
			cmd.Blit(m_TempHandle1.Identifier(), m_TempHandle2.Identifier(), m_Material, 1);
			dst = m_TempHandle2.Identifier();

			// Copy Color
			cmd.ReleaseTemporaryRT(m_TempHandle1.id);
			cmd.GetTemporaryRT(m_TempHandle1.id, targetDescriptor);
			cmd.Blit(target, m_TempHandle1.id);

			// Upsample + Blend
			cmd.SetGlobalTexture("_SourceTex", m_TempHandle1.Identifier());
			cmd.Blit(dst, target, m_Material, 2);

			cmd.ReleaseTemporaryRT(m_TempHandle1.id);
			cmd.ReleaseTemporaryRT(m_TempHandle2.id);
		}
	}
}