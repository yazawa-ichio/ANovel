using ANovel.Core;
using ANovel.Service;
using System.Linq;

namespace ANovel.Commands
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

		protected override void UpdateEnvData(IEnvData data)
		{
			data = PrefixedEnvData.Get<ImageService>(data);
			if (!data.Has<ImageObjectEnvData>(m_Name))
			{
				data.Set(m_Name, new ImageObjectEnvData(m_Transition));
				LayoutConfig.SetEvnData(m_Name, data, m_Layout);
			}
			else
			{
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
			m_PlayHandle = Service.Show(m_Name, m_Transition, m_Layout);
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

		protected override void UpdateEnvData(IEnvData data)
		{
			data = PrefixedEnvData.Get<ImageService>(data);
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
			m_PlayHandle = Service.Change(m_Name, m_Transition);
		}

	}

	[CommandName("image_hide")]
	public class HideImageCommand : ImageCommandBase
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
			data = PrefixedEnvData.Get<ImageService>(data);
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
			m_PlayHandle = Service.Hide(m_Name, m_Transition);
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
			data = PrefixedEnvData.Get<ImageService>(data);
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
			var handles = m_Names.Select(x => Service.Hide(x, m_Transition)).ToArray();
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

		protected override void UpdateEnvData(IEnvData data)
		{
			data = PrefixedEnvData.Get<ImageService>(data);
			if (data.Has<ImageObjectEnvData>(m_Name))
			{
				LayoutConfig.UpdateEvnData(m_Name, data, m_Layout);
			}
		}

		protected override void Execute()
		{
			m_PlayHandle = Service.PlayAnim(m_Name, m_Config, m_Layout);
		}

	}

}