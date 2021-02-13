using ANovel.Core;
using System;
using UnityEngine;

namespace ANovel.Service
{
	public class BGService : Service
	{
		public static readonly string EnvKey = "BG";

		public override Type ServiceType => typeof(BGService);

		IScreenService Screen => Container.Get<IScreenService>();

		ImageBuffer m_Buffer;

		protected override void Initialize()
		{
			Screen.OnDeleteChild += OnDeleteChild;
			Screen.OnBeginSwap += OnBeginSwap;
			m_Buffer = new ImageBuffer(Container);
		}

		void OnBeginSwap(BeginSwapEvent e)
		{
			m_Buffer.OnBeginSwap(e);
		}

		void OnDeleteChild(DeleteChildEvent e)
		{
			m_Buffer.Delete(e.TargetId);
		}

		public IPlayHandle Show(ImageObjectConfig transition, LayoutConfig layout)
		{
			return m_Buffer.Show(transition, layout);
		}

		public IPlayHandle Change(ImageObjectConfig transition)
		{
			return m_Buffer.Change(transition);
		}

		public IPlayHandle Hide(ImageObjectConfig transition)
		{
			return m_Buffer.Hide(transition);
		}

		public IPlayHandle PlayAnim(PlayAnimConfig config, LayoutConfig layout)
		{
			return m_Buffer.PlayAnim(config, layout);
		}

		protected override void PreRestore(IEnvDataHolder data, IPreLoader loader)
		{
			data = PrefixedEnvData.Get<BGService>(data);
			if (data.TryGet<ImageObjectEnvData>(EnvKey, out var value))
			{
				loader.Load<Texture>(Path.GetBg(value.Path));
			}
		}

		protected override void Restore(IEnvDataHolder data, ResourceCache cache)
		{
			data = PrefixedEnvData.Get<BGService>(data);
			if (data.TryGet<ImageObjectEnvData>(EnvKey, out var value))
			{
				var size = data.Get<LayoutConfig.LayoutSizeEnvData>(EnvKey);
				var config = ImageObjectConfig.Restore(value, Path.BgRoot, cache);
				var layout = LayoutConfig.Restore(EnvKey, data);
				Show(config, layout);
			}
		}

	}
}