using System;
using UnityEngine;

namespace ANovel
{

	public class UIImageMaterial : IDisposable
	{
		public static readonly string ShaderName = "ANovel/UIImage";

		static readonly string TRANS_RULE = "_ANOVEL_TRANS_RULE";

		static readonly int s_MainTexId;
		static readonly int s_BackTexId;
		static readonly int s_RuleTexId;
		static readonly int s_ColorId;
		static readonly int s_BackColorId;
		static readonly int s_TransValueId;
		static readonly int s_RuleVagueId;
		static readonly int s_RuleAlphaId;

		static UIImageMaterial()
		{
			s_MainTexId = Shader.PropertyToID("_MainTex");
			s_BackTexId = Shader.PropertyToID("_BackTex");
			s_RuleTexId = Shader.PropertyToID("_RuleTex");
			s_ColorId = Shader.PropertyToID("_Color");
			s_BackColorId = Shader.PropertyToID("_BackColor");
			s_TransValueId = Shader.PropertyToID("_TransValue");
			s_RuleVagueId = Shader.PropertyToID("_RuleVague");
			s_RuleAlphaId = Shader.PropertyToID("_RuleAlpha");
		}

		public Texture MainTex
		{
			set
			{
				if (value == null)
				{
					m_Material.SetTexture(s_MainTexId, Texture2D.blackTexture);
				}
				else
				{
					m_Material.SetTexture(s_MainTexId, value);
				}
			}
			get
			{
				var tex = m_Material.GetTexture(s_MainTexId);
				if (tex == Texture2D.blackTexture)
				{
					return null;
				}
				return tex;
			}
		}

		public Texture BackTex
		{
			set => m_Material.SetTexture(s_BackTexId, value);
			get => m_Material.GetTexture(s_BackTexId);
		}

		public Texture RuleTex
		{
			set
			{
				m_Material.SetTexture(s_RuleTexId, value);
				if (value != null)
				{
					m_Material.EnableKeyword(TRANS_RULE);
					if (value is Texture2D tex && (tex.format == TextureFormat.Alpha8))
					{
						m_Material.SetInt(s_RuleAlphaId, 1);
					}
					else
					{
						m_Material.SetInt(s_RuleAlphaId, 0);
					}
				}
				else
				{
					m_Material.DisableKeyword(TRANS_RULE);
				}
			}
			get => m_Material.GetTexture(s_RuleTexId);
		}

		public Color Color
		{
			set { m_Color = value; m_Material.SetColor(s_ColorId, value); }
			get => m_Color;
		}

		public Color BackColor
		{
			set { m_BackColor = value; m_Material.SetColor(s_BackColorId, value); }
			get => m_BackColor;
		}

		public float TransValue
		{
			set { m_TransValue = value; m_Material.SetFloat(s_TransValueId, value); }
			get => m_TransValue;
		}

		public float RuleVague
		{
			set { m_RuleVague = value; m_Material.SetFloat(s_RuleVagueId, value); }
			get => m_RuleVague;
		}

		public Material RawMaterial => m_Material;

		Material m_Material;
		Color m_Color = Color.white;
		Color m_BackColor = Color.white;
		float m_TransValue;
		float m_RuleVague;

		public UIImageMaterial()
		{
			m_Material = new Material(ShaderCache.Get(ShaderName));
		}

		public UIImageMaterial(Material material)
		{
			m_Material = material;
		}

		public void BackToMain()
		{
			Color = BackColor;
			MainTex = BackTex;
			BackColor = Color.white;
			BackTex = null;
		}

		public void Reset()
		{
			MainTex = null;
			BackTex = null;
			RuleTex = null;
			Color = Color.white;
			BackColor = Color.white;
			TransValue = 0;
		}

		public void Copy(UIImageMaterial src)
		{
			MainTex = src.MainTex;
			BackTex = src.BackTex;
			RuleTex = src.RuleTex;
			Color = src.Color;
			BackColor = src.BackColor;
			TransValue = src.TransValue;
			RuleVague = src.RuleVague;
		}

		public void Dispose()
		{
			Material.Destroy(m_Material);
			m_Material = null;
		}
	}

}