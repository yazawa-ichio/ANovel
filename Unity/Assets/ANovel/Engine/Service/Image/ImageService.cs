using ANovel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANovel.Service
{
	public class ImageService : Service
	{

		public override Type ServiceType => typeof(ImageService);

		IScreenService Screen => Container.Get<IScreenService>();

		Dictionary<string, ImageBuffer> m_Images = new Dictionary<string, ImageBuffer>();

		protected override void Initialize()
		{
			Screen.OnDeleteChild += OnDeleteChild;
			Screen.OnBeginSwap += OnBeginSwap;
		}

		void OnBeginSwap(BeginSwapEvent e)
		{
			foreach (var image in m_Images.Values)
			{
				image.OnBeginSwap(e);
			}
		}

		void OnDeleteChild(DeleteChildEvent e)
		{
			foreach (var image in m_Images.Values)
			{
				image.Delete(e.TargetId);
			}
		}

		public IPlayHandle Show(string name, ImageObjectConfig config, LayoutConfig layout)
		{
			if (!m_Images.TryGetValue(name, out var buffer))
			{
				m_Images[name] = buffer = new ImageBuffer(Container);
			}
			return buffer.Show(config, layout);
		}

		public IPlayHandle Change(string name, ImageObjectConfig config)
		{
			if (!m_Images.TryGetValue(name, out var buffer))
			{
				m_Images[name] = buffer = new ImageBuffer(Container);
			}
			return buffer.Change(config);
		}

		public IPlayHandle Hide(string name, ImageObjectConfig config)
		{
			if (!m_Images.TryGetValue(name, out var buffer))
			{
				m_Images[name] = buffer = new ImageBuffer(Container);
			}
			return buffer.Hide(config);
		}

		public IPlayHandle PlayAnim(string name, PlayAnimConfig config, LayoutConfig layout)
		{
			if (m_Images.TryGetValue(name, out var buffer))
			{
				return buffer.PlayAnim(config, layout);
			}
			throw new Exception($"image not found {name}");
		}

		protected override void PreRestore(IEnvDataHolder data, IPreLoader loader)
		{
			data = PrefixedEnvData.Get<ImageService>(data);
			foreach (var kvp in data.GetAll<ImageObjectEnvData>())
			{
				loader.Load<Texture>(Path.GetImage(kvp.Value.Path));
			}
		}

		protected override void Restore(IEnvDataHolder data, ResourceCache cache)
		{
			data = PrefixedEnvData.Get<ImageService>(data);
			foreach (var kvp in data.GetAll<ImageObjectEnvData>())
			{
				var config = ImageObjectConfig.Restore(kvp.Value, Path.ImageRoot, cache);
				var layout = LayoutConfig.Restore(kvp.Key, data);
				Show(kvp.Key, config, layout);
			}
		}

	}

}