using UnityEngine;
using UnityEngine.Rendering;

namespace ANovel.Engine.PostEffects
{
	public interface IPostEffect
	{
		bool Enabled { get; }
		bool IsParam(object param);
		void SetCameraColor(RenderTargetIdentifier target);
		void Execute(object param, CommandBuffer cmd, RenderTextureDescriptor targetDescriptor, RenderTargetIdentifier target);
		void OnValidate();
	}
}