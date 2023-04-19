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
		const int MaxIteration = 16;


		Material m_Material;
		RenderTargetHandle m_TempHandle;
		int m_IntensityId;


		RenderTargetHandle[] m_TempDownHandle = new RenderTargetHandle[MaxIteration];
		RenderTargetHandle[] m_TempUpHandle = new RenderTargetHandle[MaxIteration];

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
				m_TempHandle.Init("_TempRT");
				m_IntensityId = Shader.PropertyToID("_Intensity");
				for (int i = 0; i < MaxIteration; i++)
				{
					m_TempDownHandle[i].Init("_TempDownRT" + i);
					m_TempUpHandle[i].Init("_TempUpRT" + i);
				}
			}
			m_Material.SetFloat(m_IntensityId, param.Intensity);
			m_Material.SetFloat("_Threshold", param.Threshold);
			m_Material.SetFloat("_ThresholdKnee", param.Threshold * 0.5f);
			m_Material.SetFloat("_Distance", 2f);

			var descriptor = targetDescriptor;
			descriptor.depthBufferBits = 0;
			//descriptor.colorFormat = RenderTextureFormat.R8;

			// Prefilter
			cmd.GetTemporaryRT(m_TempHandle.id, descriptor);
			cmd.Blit(target, m_TempHandle.Identifier());

			cmd.GetTemporaryRT(m_TempDownHandle[0].id, descriptor);
			cmd.Blit(target, m_TempDownHandle[0].Identifier(), m_Material, 0);



			//int tw = descriptor.width >> 1;
			//int th = descriptor.height >> 1;
			int tw = descriptor.width;
			int th = descriptor.height;

			int maxSize = Mathf.Max(tw, th);
			int iterations = Mathf.FloorToInt(Mathf.Log(maxSize, 2f) - 1) - 2;
			int mipCount = Mathf.Clamp(iterations, 1, MaxIteration);

			// Downsample - gaussian pyramid
			var lastDown = m_TempDownHandle[0];
			for (int i = 1; i < mipCount; i++)
			{
				tw = Mathf.Max(1, tw >> 1);
				th = Mathf.Max(1, th >> 1);
				var mipDown = m_TempDownHandle[i];
				var mipUp = m_TempUpHandle[i];

				descriptor.width = tw;
				descriptor.height = th;

				cmd.GetTemporaryRT(m_TempDownHandle[i].id, descriptor, FilterMode.Bilinear);
				cmd.GetTemporaryRT(m_TempUpHandle[i].id, descriptor, FilterMode.Bilinear);

				// Classic two pass gaussian blur - use mipUp as a temporary target
				//   First pass does 2x downsampling + 9-tap gaussian
				//   Second pass does 9-tap gaussian using a 5-tap filter + bilinear filtering
				cmd.Blit(lastDown.id, mipUp.id, m_Material, 1);
				cmd.Blit(mipUp.id, mipDown.id, m_Material, 2);

				lastDown = mipDown;
			}

			for (int i = mipCount - 2; i >= 0; i--)
			{
				var lowMip = (i == mipCount - 2) ? m_TempDownHandle[i + 1] : m_TempUpHandle[i + 1];
				var highMip = m_TempDownHandle[i];
				var dst = m_TempUpHandle[i];

				cmd.SetGlobalTexture("_BloomTex", lowMip.id);
				cmd.Blit(highMip.id, dst.id, m_Material, 3);
			}

			cmd.SetGlobalTexture("_BloomTex", m_TempUpHandle[0].id);
			cmd.Blit(m_TempHandle.id, target, m_Material, 4);

			cmd.ReleaseTemporaryRT(m_TempHandle.id);
			for (int i = 0; i < mipCount; i++)
			{
				cmd.ReleaseTemporaryRT(m_TempDownHandle[i].id);
				cmd.ReleaseTemporaryRT(m_TempUpHandle[i].id);
			}

			/*

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
			*/
		}
	}
}