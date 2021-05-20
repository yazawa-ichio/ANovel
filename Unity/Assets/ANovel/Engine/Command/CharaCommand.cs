using System.Linq;
using Category = ANovel.Engine.ImageService.Category;

namespace ANovel.Engine
{

	public class CharaCommandBase : SyncCommandBase, IUseTransitionScope
	{
		protected ImageService Service => Get<ImageService>();

		protected MessageService Message => Get<MessageService>();

		bool IUseTransitionScope.Use => true;

		protected CharaMetaData GetMetaData(string name)
		{
			return CharaMetaData.Get(Meta, name);
		}

	}


	[CommandName("chara")]
	public class CharaShowCommand : CharaCommandBase
	{
		[CommandField(Required = true)]
		string m_Name = null;
		[InjectParam]
		CharaObjectConfig m_Config = new CharaObjectConfig();
		[InjectParam(IgnoreKey = "path")]
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
			data = data.Prefixed(Category.Chara);
			var meta = GetMetaData(m_Name);
			m_Config.Init(m_Name, meta, data);
			m_Transition.Path = m_Config.ImagePath;
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
			meta.UpdateLayout(data.Get<CharaObjectEnvData>(m_Name), m_Layout);
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadTexture(Path.CharaRoot, loader);
			m_Transition.PreloadRule(Path, loader);
			m_Config?.FaceWindowConfig?.PreloadTexture(Path.CharaFaceWindowRoot, loader);
		}

		protected override void Execute()
		{
			m_Transition.LoadTexture(Path.CharaRoot, Cache);
			m_Transition.LoadRule(Path, Cache);
			if (m_Config.FaceWindowConfig != null)
			{
				m_Config.FaceWindowConfig.LoadTexture(Path.CharaFaceWindowRoot, Cache);
				Event.Publish(FaceWindowEvent.Update, m_Config.FaceWindowConfig);
			}
			m_PlayHandle = Service.Show(Category.Chara, m_Name, m_Transition, m_Layout);
		}
	}

	[CommandName("chara_face_window")]
	public class CharaParamCommand : Command
	{
		protected PathConfig Path => Get<EngineConfig>().Path;

		protected ImageService Service => Get<ImageService>();

		[CommandField(Required = true)]
		string m_Name = null;
		[InjectParam(IgnoreKey = nameof(CharaObjectConfig.FaceWindow))]
		CharaObjectConfig m_Config = new CharaObjectConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Chara);
			m_Config.Init(m_Name, CharaMetaData.Get(Meta, m_Name), data);
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Config.FaceWindowConfig.PreloadTexture(Path.CharaFaceWindowRoot, loader);
		}

		protected override void Execute()
		{
			m_Config.FaceWindowConfig.LoadTexture(Path.CharaFaceWindowRoot, Cache);
			Event.Publish(FaceWindowEvent.Update, m_Config.FaceWindowConfig);
		}
	}

	[CommandName("chara_change")]
	public class CharaChangeCommand : CharaCommandBase
	{
		[CommandField(Required = true)]
		string m_Name = null;
		[InjectParam]
		CharaObjectConfig m_Config = new CharaObjectConfig();
		[InjectParam(IgnoreKey = "path")]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.1f)
		};
		[CommandField]
		bool m_Front = false;

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Chara);
			m_Config.Init(m_Name, GetMetaData(m_Name), data);
			m_Transition.Path = m_Config.ImagePath;
			if (m_Front)
			{
				m_Transition.AutoOrder = ScreenOrderEnvData.GenOrder(data);
			}
			data.Update<ImageObjectEnvData, ImageObjectConfig>(m_Name, m_Transition);
		}

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadTexture(Path.CharaRoot, loader);
			m_Transition.PreloadRule(Path, loader);
			m_Config.FaceWindowConfig?.PreloadTexture(Path.CharaFaceWindowRoot, loader);
		}

		protected override void Execute()
		{
			m_Transition.LoadTexture(Path.CharaRoot, Cache);
			m_Transition.LoadRule(Path, Cache);
			if (m_Config.FaceWindowConfig != null)
			{
				m_Config.FaceWindowConfig.LoadTexture(Path.CharaFaceWindowRoot, Cache);
				Event.Publish(FaceWindowEvent.Update, m_Config.FaceWindowConfig);
			}
			m_PlayHandle = Service.Change(Category.Chara, m_Name, m_Transition);
		}

	}

	[CommandName("chara_hide")]
	public class CharaHideCommand : CharaCommandBase
	{
		[CommandField(Required = true)]
		string m_Name = null;
		[CommandField]
		bool m_Clear = true;
		[InjectParam(IgnoreKey = "path")]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.15f)
		};

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Chara);
			if (m_Clear)
			{
				data.Delete<CharaObjectEnvData>(m_Name);
			}
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
			m_PlayHandle = Service.Hide(Category.Chara, m_Name, m_Transition);
		}

	}

	[CommandName("chara_hide_all")]
	public class CharaHideAllCommand : CharaCommandBase
	{
		[InjectParam(IgnoreKey = "path")]
		ImageObjectConfig m_Transition = new ImageObjectConfig()
		{
			Time = Millisecond.FromSecond(0.15f)
		};
		[CommandField]
		bool m_Clear = true;
		[CommandField]
		string m_Level = null;

		string[] m_Names;

		protected override void Preload(IPreLoader loader)
		{
			m_Transition.PreloadRule(Path, loader);
		}

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category.Chara);
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
				if (m_Clear)
				{
					data.Delete<CharaObjectEnvData>(name);
				}
				data.Delete<ImageObjectEnvData>(name);
				LayoutConfig.DeleteEvnData(name, data);
			}
		}

		protected override void Execute()
		{
			m_Transition.LoadRule(Path, Cache);
			var handles = m_Names.Select(x => Service.Hide(Category.Chara, x, m_Transition)).ToArray();
			m_PlayHandle = new CombinePlayHandle(handles);
		}

	}

	[CommandName("chara_control")]
	public class CharaControlCommand : CharaCommandBase
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
			data = data.Prefixed(Category.Chara);
			if (data.TryGet<ImageObjectEnvData>(m_Name, out var image))
			{
				LayoutConfig.UpdateEvnData(m_Name, data, m_Layout);
				var meta = GetMetaData(m_Name);
				meta.UpdateLayout(data.Get<CharaObjectEnvData>(m_Name), m_Layout);
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
			m_PlayHandle = Service.PlayAnim(Category.Chara, m_Name, m_Config, m_Layout);
		}

	}

}