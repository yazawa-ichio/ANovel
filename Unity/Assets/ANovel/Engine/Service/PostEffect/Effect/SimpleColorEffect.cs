using System.Globalization;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ANovel.Engine.PostEffects
{
	public struct SimpleColorEffectParam : IPostEffectParam
	{
		public static readonly SimpleColorEffectParam Default = new SimpleColorEffectParam()
		{
			Rate = 1f,
		};

		public enum EffectType
		{
			Grayscale,
			Sepia,
			Negative,
		}

		[Argument(Required = true)]
		public EffectType Type;

		[Range(0f, 1f)]
		public float Rate;
	}

	public class SimpleColorEffect : PostEffectBase<SimpleColorEffectParam>
	{
		Material m_Material;
		RenderTargetHandle m_TempHandle;
		int m_RateId;
		SimpleColorEffectParam.EffectType m_PrevType;

		[SerializeField]
		protected Shader m_Shader;

		protected override void OnValidate()
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("ANovel/ColorEffect");
			}
		}

		protected override void Execute(SimpleColorEffectParam param, CommandBuffer cmd, RenderTextureDescriptor targetDescriptor, RenderTargetIdentifier target)
		{
			if (m_Material == null)
			{
				m_Material = new Material(m_Shader);
				m_Material.DisableKeyword("_ANOVEL_COLOR_CHANGE_" + SimpleColorEffectParam.EffectType.Grayscale.ToString().ToUpper(CultureInfo.InvariantCulture));
				m_TempHandle.Init("_TempRT");
				m_RateId = Shader.PropertyToID("_Rate");
				m_PrevType = SimpleColorEffectParam.EffectType.Grayscale;
			}

			if (m_PrevType != param.Type)
			{
				m_Material.DisableKeyword("_ANOVEL_COLOR_CHANGE_" + m_PrevType.ToString().ToUpper(CultureInfo.InvariantCulture));
				m_Material.EnableKeyword("_ANOVEL_COLOR_CHANGE_" + param.Type.ToString().ToUpper(CultureInfo.InvariantCulture));
				m_PrevType = param.Type;
			}

			m_Material.EnableKeyword("_ANOVEL_COLOR_CHANGE_" + param.Type.ToString().ToUpper(CultureInfo.InvariantCulture));
			m_Material.SetFloat(m_RateId, param.Rate);
			cmd.GetTemporaryRT(m_TempHandle.id, targetDescriptor);
			cmd.Blit(target, m_TempHandle.Identifier(), m_Material);
			cmd.Blit(m_TempHandle.Identifier(), target);
			cmd.ReleaseTemporaryRT(m_TempHandle.id);
		}
	}
}