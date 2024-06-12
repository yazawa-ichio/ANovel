using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ANovel.Engine.PostEffects
{

	public struct ChromaticAberrationParam : IPostEffectParam
	{
		public static readonly ChromaticAberrationParam Default = new()
		{
		};

		[Range(-1f, 1f), RateArgument]
		public float X;
		[Range(-1f, 1f), RateArgument]
		public float Y;
		[Range(0f, 1f), RateArgument]
		public float Around;
		[Range(0f, 1f), RateArgument]
		public float Center;

	}

	[Serializable]
	public class ChromaticAberration : PostEffectBase<ChromaticAberrationParam>
	{
		Material m_Material;
		int m_TempHandle;
		int m_ChromaPositionId;
		int m_ChromaAmountCenterId;
		int m_ChromaAmountAroundId;

		[SerializeField]
		protected Shader m_Shader;

		protected override void OnValidate()
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("ANovel/ChromaticAberration");
			}
		}

		protected override void Execute(ChromaticAberrationParam param, CommandBuffer cmd, RenderTextureDescriptor targetDescriptor, RenderTargetIdentifier target)
		{
			if (m_Material == null)
			{
				m_Material = new Material(m_Shader);
				m_TempHandle = Shader.PropertyToID("_TempRT");
				m_ChromaPositionId = Shader.PropertyToID("_ChromaPosition");
				m_ChromaAmountCenterId = Shader.PropertyToID("_ChromaAmountCenter");
				m_ChromaAmountAroundId = Shader.PropertyToID("_ChromaAmountAround");
			}

			m_Material.SetVector(m_ChromaPositionId, new Vector4(param.X, param.Y, 0, 0));
			m_Material.SetFloat(m_ChromaAmountCenterId, param.Center);
			m_Material.SetFloat(m_ChromaAmountAroundId, param.Around);

			cmd.GetTemporaryRT(m_TempHandle, targetDescriptor);
			cmd.Blit(target, m_TempHandle, m_Material);
			cmd.Blit(m_TempHandle, target);
			cmd.ReleaseTemporaryRT(m_TempHandle);

		}
	}
}