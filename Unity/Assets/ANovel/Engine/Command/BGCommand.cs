using Category = ANovel.Engine.ImageService.Category;

namespace ANovel.Engine
{

	public class BGCommandBase : SyncCommandBase, IUseTransitionScope
	{
		public static readonly string EnvKey = "BG";

		protected static readonly string LayoutLevel = ANovel.Level.BG.ToString();

		protected ImageService Service => Get<ImageService>();

		bool IUseTransitionScope.Use => true;

	}

	[CommandName("bg")]
	public class BGShowCommand : BGCommandBase
	{
		[InjectParam]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.4f)
		};
		[InjectParam(IgnoreKey = "level")]
		LayoutConfig m_Layout = new LayoutConfig()
		{
			ScreenMatch = ScreenMatchMode.Shrink,
			Level = LayoutLevel,
		};

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Bg);
			if (!data.Has<ImageObjectEnvData>(EnvKey))
			{
				// 現在の仕様ではBGは最背面固定
				//m_Transition.AutoOrder = ScreenOrderEnvData.GenOrder(data);
				data.Set(EnvKey, new ImageObjectEnvData(m_Transition));
				LayoutConfig.SetEvnData(EnvKey, data, m_Layout);
			}
			else
			{
				data.Update<ImageObjectEnvData, ImageObjectConfig>(EnvKey, m_Transition);
				LayoutConfig.UpdateEvnData(EnvKey, data, m_Layout);
			}
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadTexture(Path.BgRoot, loader);
			m_Transition.PreloadRule(Path, loader);
		}

		protected override void Execute()
		{
			m_Transition.LoadTexture(Path.BgRoot, Cache);
			m_Transition.LoadRule(Path, Cache);
			m_PlayHandle = Service.Show(Category.Bg, EnvKey, m_Transition, m_Layout);
		}

	}

	[CommandName("bg_change")]
	public class BGChangeCommand : BGCommandBase
	{
		[InjectParam]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.4f)
		};

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Bg);
			data.Update<ImageObjectEnvData, ImageObjectConfig>(EnvKey, m_Transition);
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadTexture(Path.BgRoot, loader);
			m_Transition.PreloadRule(Path, loader);
		}

		protected override void Execute()
		{
			m_Transition.LoadTexture(Path.BgRoot, Cache);
			m_Transition.LoadRule(Path, Cache);
			m_PlayHandle = Service.Change(Category.Bg, EnvKey, m_Transition);
		}

	}

	[CommandName("bg_hide")]
	public class BGHideCommand : BGCommandBase
	{
		[InjectParam(IgnoreKey = "path")]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.4f)
		};

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Bg);
			data.Delete<ImageObjectEnvData>(EnvKey);
			LayoutConfig.DeleteEvnData(EnvKey, data);
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadRule(Path, loader);
		}

		protected override void Execute()
		{
			m_Transition.LoadRule(Path, Cache);
			m_PlayHandle = Service.Hide(Category.Bg, EnvKey, m_Transition);
		}

	}

	[CommandName("bg_control")]
	public class BGControlCommand : BGCommandBase
	{
		[InjectParam]
		PlayAnimConfig m_Config = new PlayAnimConfig();
		[InjectParam(IgnoreKey = "level")]
		LayoutConfig m_Layout = new LayoutConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Bg);
			if (data.Has<ImageObjectEnvData>(EnvKey))
			{
				LayoutConfig.UpdateEvnData(EnvKey, data, m_Layout);
			}
		}

		protected override void Execute()
		{
			m_PlayHandle = Service.PlayAnim(Category.Bg, EnvKey, m_Config, m_Layout);
		}

	}


}