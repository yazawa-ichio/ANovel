using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ANovel.Engine.PostEffects
{
	public interface IPostEffectParam : IEnvValue, IScreenChildEnvData
	{

	}

	[Serializable]
	public abstract class PostEffectBase<T> : IPostEffect where T : struct, IPostEffectParam
	{

		[SerializeField]
		string m_Name;

		public string Name => m_Name;

		[SerializeField]
		bool m_Enabled = true;

		public bool Enabled
		{
			get => m_Enabled;
			set => m_Enabled = value;
		}

		public PostEffectBase()
		{
			m_Name = GetType().Name;
		}

		protected RenderTargetIdentifier CameraColorTarget;

		void IPostEffect.SetCameraColor(RenderTargetIdentifier target)
		{
			CameraColorTarget = target;
		}

		protected abstract void Execute(T param, CommandBuffer cmd, RenderTextureDescriptor targetDescriptor, RenderTargetIdentifier target);


		void IPostEffect.Execute(object param, CommandBuffer cmd, RenderTextureDescriptor targetDescriptor, RenderTargetIdentifier target)
		{
			Execute((T)param, cmd, targetDescriptor, target);
		}

		bool IPostEffect.IsParam(object param)
		{
			return IsParam(param);
		}

		protected virtual bool IsParam(object param)
		{
			return param is T;
		}

		void IPostEffect.OnValidate()
		{
			OnValidate();
		}

		protected virtual void OnValidate()
		{
		}
	}
}