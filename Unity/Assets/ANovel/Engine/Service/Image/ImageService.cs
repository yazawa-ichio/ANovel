using ANovel.Core;
using System;
using System.Collections.Generic;

namespace ANovel.Service
{
	public partial class ImageService : Service
	{
		public enum Category
		{
			Image,
			Bg,
			Chara,
		}

		public override Type ServiceType => typeof(ImageService);

		IScreenService Screen => Container.Get<IScreenService>();

		Dictionary<Category, Controllers> m_Images = new Dictionary<Category, Controllers>();

		protected override void Initialize()
		{
			m_Images[Category.Bg] = new Controllers(Container, Category.Bg);
			m_Images[Category.Image] = new Controllers(Container, Category.Image);
			m_Images[Category.Chara] = new Controllers(Container, Category.Chara);
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
				image.OnDeleteChild(e);
			}
		}

		public IPlayHandle Show(Category category, string name, ImageObjectConfig config, LayoutConfig layout)
		{
			return m_Images[category].Show(name, config, layout);
		}

		public IPlayHandle Change(Category category, string name, ImageObjectConfig config)
		{
			return m_Images[category].Change(name, config);
		}

		public IPlayHandle Hide(Category category, string name, ImageObjectConfig config)
		{
			return m_Images[category].Hide(name, config);
		}

		public IPlayHandle PlayAnim(Category category, string name, PlayAnimConfig config, LayoutConfig layout)
		{
			return m_Images[category].PlayAnim(name, config, layout);
		}

		public void SetOrder(Category category, string name, long autoOrder)
		{
			m_Images[category].SetOrder(name, autoOrder);
		}

		protected override void PreRestore(IMetaData meta, IEnvDataHolder data, IPreLoader loader)
		{
			foreach (var image in m_Images.Values)
			{
				image.PreRestore(meta, data, loader);
			}
		}

		protected override void Restore(IMetaData meta, IEnvDataHolder data, ResourceCache cache)
		{
			foreach (var image in m_Images.Values)
			{
				image.Restore(meta, data, cache);
			}
		}

	}

}