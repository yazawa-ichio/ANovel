using System.Linq;
using Category = ANovel.Engine.ImageService.Category;

namespace ANovel.Engine
{

	public class ImageCommandBase : SyncCommandBase, IUseTransitionScope
	{
		protected ImageService Service => Get<ImageService>();

		bool IUseTransitionScope.Use => true;

	}

	[CommandName("image")]
	public class ImageShowCommand : ImageCommandBase
	{
		[CommandField(Required = true)]
		string m_Name = null;
		[InjectParam]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.15f)
		};
		[InjectParam]
		LayoutConfig m_Layout = new LayoutConfig();
		[CommandField]
		bool m_Front = false;

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Image);
			if (!data.Has<ImageObjectEnvData>(m_Name))
			{
				m_Transition.AutoOrder = ScreenOrderEnvData.GenOrder(data);
				data.Set(m_Name, new ImageObjectEnvData(m_Transition));
				LayoutConfig.SetEvnData(m_Name, data, m_Layout);
			}
			else
			{
				if (m_Front)
				{
					m_Transition.AutoOrder = ScreenOrderEnvData.GenOrder(data);
				}
				data.Update<ImageObjectEnvData, ImageObjectConfig>(m_Name, m_Transition);
				LayoutConfig.UpdateEvnData(m_Name, data, m_Layout);
			}
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadTexture(Path.ImageRoot, loader);
			m_Transition.PreloadRule(Path, loader);
		}

		protected override void Execute()
		{
			m_Transition.LoadTexture(Path.ImageRoot, Cache);
			m_Transition.LoadRule(Path, Cache);
			m_PlayHandle = Service.Show(Category.Image, m_Name, m_Transition, m_Layout);
		}

	}

	[CommandName("image_change")]
	public class ImageChangeCommand : ImageCommandBase
	{
		[CommandField(Required = true)]
		string m_Name = null;
		[InjectParam]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.1f)
		};
		[CommandField]
		bool m_Front = false;

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Image);
			if (m_Front)
			{
				m_Transition.AutoOrder = ScreenOrderEnvData.GenOrder(data);
			}
			data.Update<ImageObjectEnvData, ImageObjectConfig>(m_Name, m_Transition);
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadTexture(Path.ImageRoot, loader);
			m_Transition.PreloadRule(Path, loader);
		}

		protected override void Execute()
		{
			m_Transition.LoadTexture(Path.ImageRoot, Cache);
			m_Transition.LoadRule(Path, Cache);
			m_PlayHandle = Service.Change(Category.Image, m_Name, m_Transition);
		}

	}

	[CommandName("image_hide")]
	public class ImageHideCommand : ImageCommandBase
	{
		[CommandField(Required = true)]
		string m_Name = null;

		[InjectParam(IgnoreKey = "path")]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.15f)
		};

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Image);
			data.Delete<ImageObjectEnvData>(m_Name);
			LayoutConfig.DeleteEvnData(m_Name, data);
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadRule(Path, loader);
		}

		protected override void Execute()
		{
			m_Transition.LoadRule(Path, Cache);
			m_PlayHandle = Service.Hide(Category.Image, m_Name, m_Transition);
		}

	}

	[CommandName("image_hide_all")]
	public class ImageHideAllCommand : ImageCommandBase
	{
		[InjectParam(IgnoreKey = "path")]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.15f)
		};
		[CommandField]
		string m_Level = null;

		string[] m_Names;

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadRule(Path, loader);
		}

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Image);
			m_Names = data.GetKeys<LayoutConfig.LayoutLevelEnvData>(x =>
			{
				if (string.IsNullOrEmpty(m_Level))
				{
					return true;
				}
				var level = x.Level;
				if (string.IsNullOrEmpty(level))
				{
					level = Level.Center.ToString();
				}
				return level == m_Level;
			}).ToArray();
			foreach (var name in m_Names)
			{
				data.Delete<ImageObjectEnvData>(name);
				LayoutConfig.DeleteEvnData(name, data);
			}
		}

		protected override void Execute()
		{
			m_Transition.LoadRule(Path, Cache);
			var handles = m_Names.Select(x => Service.Hide(Category.Image, x, m_Transition)).ToArray();
			m_PlayHandle = new CombinePlayHandle(handles);
		}

	}

	[CommandName("image_control")]
	public class ImageControlCommand : ImageCommandBase
	{
		[CommandField]
		string m_Name = null;
		[InjectParam]
		PlayAnimConfig m_Config = new PlayAnimConfig();
		[InjectParam(IgnoreKey = "level")]
		LayoutConfig m_Layout = new LayoutConfig();
		[CommandField]
		bool m_Front = false;
		long m_AutoOrder;

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Image);
			if (data.TryGet<ImageObjectEnvData>(m_Name, out var image))
			{
				LayoutConfig.UpdateEvnData(m_Name, data, m_Layout);
				if (m_Front)
				{
					image.AutoOrder = m_AutoOrder = ScreenOrderEnvData.GenOrder(data);
					data.Set(m_Name, image);
				}
			}
		}

		protected override void Execute()
		{
			if (m_Front)
			{
				Service.SetOrder(Category.Image, m_Name, m_AutoOrder);
			}
			m_PlayHandle = Service.PlayAnim(Category.Image, m_Name, m_Config, m_Layout);
		}

	}

}