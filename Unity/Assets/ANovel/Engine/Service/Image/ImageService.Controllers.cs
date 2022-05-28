using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANovel.Engine
{
	public partial class ImageService
	{
		class Controllers
		{
			Category m_Category;
			Dictionary<string, ImageController> m_Images = new Dictionary<string, ImageController>();

			ServiceContainer m_Container;

			PathConfig Path => m_Container.Get<EngineConfig>().Path;

			public Controllers(ServiceContainer container, Category category)
			{
				m_Container = container;
				m_Category = category;
			}

			public void OnBeginSwap(BeginSwapEvent e)
			{
				foreach (var image in m_Images.Values)
				{
					image.OnBeginSwap(e);
				}
			}

			public void OnDeleteChild(DeleteChildEvent e)
			{
				foreach (var image in m_Images.Values)
				{
					image.Delete(e.TargetId);
				}
			}

			public IPlayHandle Show(string name, ImageObjectConfig config, LayoutConfig layout)
			{
				if (!m_Images.TryGetValue(name, out var image))
				{
					m_Images[name] = image = new ImageController(m_Container);
				}
				return image.Show(config, layout);
			}

			public IPlayHandle Change(string name, ImageObjectConfig config)
			{
				if (!m_Images.TryGetValue(name, out var image))
				{
					m_Images[name] = image = new ImageController(m_Container);
				}
				return image.Change(config);
			}

			public IPlayHandle Hide(string name, ImageObjectConfig config)
			{
				if (!m_Images.TryGetValue(name, out var image))
				{
					m_Images[name] = image = new ImageController(m_Container);
				}
				return image.Hide(config);
			}

			public IPlayHandle PlayAnim(string name, PlayAnimConfig config, LayoutConfig layout)
			{
				if (m_Images.TryGetValue(name, out var image))
				{
					return image.PlayAnim(config, layout);
				}
				throw new Exception($"image not found {name}");
			}

			public void SetOrder(string name, long autoOrder)
			{
				if (m_Images.TryGetValue(name, out var image))
				{
					image.SetOrder(autoOrder);
				}
			}

			string GetRootPath()
			{
				switch (m_Category)
				{
					case Category.Bg:
						return Path.GetRoot(PathCategory.Bg);
					case Category.Image:
						return Path.GetRoot(PathCategory.Image);
					case Category.Chara:
						return Path.GetRoot(PathCategory.Chara);
				}
				return Path.GetRoot(PathCategory.Image);
			}

			public void PreRestore(IMetaData meta, IEnvDataHolder data, IPreLoader loader)
			{
				data = data.Prefixed(m_Category);
				foreach (var kvp in data.GetAll<ImageObjectEnvData>())
				{
					loader.Load<Texture>(Path.GetPath(GetRootPath(), kvp.Value.Path));
				}
			}

			public void Restore(IMetaData meta, IEnvDataHolder data, IResourceCache cache)
			{
				data = data.Prefixed(m_Category);
				foreach (var kvp in data.GetAll<ImageObjectEnvData>())
				{
					var config = ImageObjectConfig.Restore(kvp.Value, GetRootPath(), cache);
					var layout = LayoutConfig.Restore(kvp.Key, data);
					if (m_Category == Category.Chara)
					{
						var chara = data.Get<CharaObjectEnvData>(kvp.Key);
						CharaMetaData.Get(meta, kvp.Key).UpdateLayout(chara, layout);
					}
					Show(kvp.Key, config, layout);
				}
			}

		}

	}



}