using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ANovel.Engine.PostEffects
{
	public struct BlurParam : IPostEffectParam
	{
		public static readonly BlurParam Default = new BlurParam()
		{
			Distance = 1f,
			Deviation = 10f
		};

		[Range(0f, 10f)]
		public float Distance;
		[Range(1f, 20f)]
		public float Deviation;
	}

	[Serializable]
	public class Blur : PostEffectBase<BlurParam>
	{

		Material m_Material;
		RenderTargetHandle m_TempHandle;
		int m_SamplingDistanceId;
		int m_DeviationId;

		[SerializeField]
		protected Shader m_Shader;

		protected override void OnValidate()
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("ANovel/Blur");
			}
		}

		protected override void Execute(BlurParam param, CommandBuffer cmd, RenderTextureDescriptor targetDescriptor, RenderTargetIdentifier target)
		{
			if (m_Material == null)
			{
				m_Material = new Material(m_Shader);
				m_TempHandle.Init("_TempRT");
				m_SamplingDistanceId = Shader.PropertyToID("_SamplingDistance");
				m_DeviationId = Shader.PropertyToID("_Deviation");
			}
			m_Material.SetFloat(m_SamplingDistanceId, param.Distance);
			m_Material.SetFloat(m_DeviationId, param.Deviation);

			var descriptor = targetDescriptor;
			descriptor.depthBufferBits = 0;
			descriptor.width = (int)(descriptor.width / 2f);
			descriptor.height = (int)(descriptor.height / 2f);
			cmd.GetTemporaryRT(m_TempHandle.id, descriptor);

			cmd.Blit(target, m_TempHandle.Identifier(), m_Material);
			cmd.Blit(m_TempHandle.Identifier(), target);

			cmd.ReleaseTemporaryRT(m_TempHandle.id);
		}
	}
}