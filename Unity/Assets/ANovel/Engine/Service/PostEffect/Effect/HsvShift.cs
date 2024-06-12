using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ANovel.Engine.PostEffects
{
	[Serializable]
	public struct HsvShiftParam : IPostEffectParam
	{
		public static readonly HsvShiftParam Default = new()
		{
			H = 0,
			S = 1f,
			V = 1f,
			Rate = 1f,
		};

		[Range(-180f, 180f)]
		public float H;
		[Range(0f, 1f), RateArgument]
		public float S;
		[Range(0f, 1f), RateArgument]
		public float V;
		[Range(0f, 1f), RateArgument]
		public float Rate;
	}

	[Serializable]
	public class HsvShift : PostEffectBase<HsvShiftParam>
	{

		Material m_Material;
		int m_TempHandle;
		int m_RateId;
		int m_HsvId;

		[SerializeField]
		protected Shader m_Shader;

		protected override void OnValidate()
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("ANovel/ColorEffect");
			}
		}

		protected override void Execute(HsvShiftParam param, CommandBuffer cmd, RenderTextureDescriptor targetDescriptor, RenderTargetIdentifier target)
		{
			if (m_Material == null)
			{
				m_Material = new Material(m_Shader);
				m_TempHandle = Shader.PropertyToID("_TempRT");
				m_RateId = Shader.PropertyToID("_Rate");
				m_HsvId = Shader.PropertyToID("_HSV");
				m_Material.DisableKeyword("_ANOVEL_COLOR_CHANGE_GRAYSCALE");
				m_Material.EnableKeyword("_ANOVEL_COLOR_CHANGE_HSV");
			}
			m_Material.SetFloat(m_RateId, param.Rate);
			m_Material.SetVector(m_HsvId, new Vector4(param.H / 360f, param.S, param.V));

			cmd.GetTemporaryRT(m_TempHandle, targetDescriptor);
			cmd.Blit(target, m_TempHandle, m_Material);
			cmd.Blit(m_TempHandle, target);
			cmd.ReleaseTemporaryRT(m_TempHandle);
		}
	}
}