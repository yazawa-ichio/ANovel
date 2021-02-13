using ANovel.Core;
using ANovel.Service;

namespace ANovel.Commands
{

	public class BGCommandBase : SyncCommandBase, IUseTransitionScope
	{
		public static readonly string EnvKey = BGService.EnvKey;

		protected static readonly string LayoutLevel = ANovel.Level.BG.ToString();

		protected BGService Service => Get<BGService>();

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
			data = PrefixedEnvData.Get<BGService>(data);
			if (!data.Has<ImageObjectEnvData>(EnvKey))
			{
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
			m_PlayHandle = Service.Show(m_Transition, m_Layout);
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
			data = PrefixedEnvData.Get<BGService>(data);
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
			m_PlayHandle = Service.Change(m_Transition);
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
			data = PrefixedEnvData.Get<BGService>(data);
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
			m_PlayHandle = Service.Hide(m_Transition);
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
			data = PrefixedEnvData.Get<BGService>(data);
			if (data.Has<ImageObjectEnvData>(EnvKey))
			{
				LayoutConfig.UpdateEvnData(EnvKey, data, m_Layout);
			}
		}

		protected override void Execute()
		{
			m_PlayHandle = Service.PlayAnim(m_Config, m_Layout);
		}

	}


}