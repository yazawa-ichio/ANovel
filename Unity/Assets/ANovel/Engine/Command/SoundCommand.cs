
namespace ANovel.Engine
{
	public abstract class SoundCommand : SyncCommandBase
	{
		protected ISoundService Sound => Get<ISoundService>();
	}

	[TagName("se")]
	public class SePlayCommand : SoundCommand
	{
		[InjectArgument]
		SeConfig m_Config = new SeConfig();
		[InjectArgument]
		[InjectPathDefine("path", PathCategory.Se)]
		PlayConfig m_PlayConfig = PlayConfig.Se;

		protected override void UpdateEnvData(IEnvData data)
		{
			if (string.IsNullOrEmpty(m_Config.Slot))
			{
				return;
			}
			data = data.Prefixed<SeConfig>();
			if (m_PlayConfig.Loop)
			{
				data.Set(m_Config.Slot, new PlaySoundEnvData(m_Config.Group, m_PlayConfig));
			}
			else
			{
				data.Delete<PlaySoundEnvData>(m_Config.Slot);
			}
		}

		protected override void Preload(IPreLoader loader)
		{
			m_PlayConfig.Preload(Path.GetRoot(PathCategory.Se), loader);
		}

		protected override void Execute()
		{
			m_PlayConfig.Load(Path.GetRoot(PathCategory.Se), Cache);
			m_PlayHandle = Sound.PlaySe(m_Config, m_PlayConfig);
		}
	}

	[TagName("se_stop")]
	public class SeStopCommand : SoundCommand
	{
		[Argument(Required = true)]
		string m_Slot = null;
		[InjectArgument]
		StopConfig m_Config = new StopConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			if (string.IsNullOrEmpty(m_Slot))
			{
				return;
			}
			data = data.Prefixed<SeConfig>();
			data.Delete<PlaySoundEnvData>(m_Slot);
		}

		protected override void Execute()
		{
			m_PlayHandle = Sound.StopSe(m_Slot, m_Config);
		}
	}

	[TagName("se_stop_all")]
	public class SeStopAllCommand : SoundCommand
	{
		[InjectArgument]
		StopConfig m_Config = new StopConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed<SeConfig>();
			data.DeleteAll<PlaySoundEnvData>();
		}

		protected override void Execute()
		{
			m_PlayHandle = Sound.StopAllSe(m_Config);
		}
	}


	[TagName("se_volume")]
	public class SeVolumeCommand : SoundCommand
	{
		[Argument(Required = true)]
		string m_Slot = null;
		[InjectArgument]
		VolumeConfig m_Config = new VolumeConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed<SeConfig>();
			data.Update<PlaySoundEnvData, VolumeConfig>(m_Slot, m_Config);
		}

		protected override void Execute()
		{
			m_PlayHandle = Sound.ChangeBgmVolume(m_Slot, m_Config);
		}

	}


	[TagName("bgm")]
	public class BgmPlayCommand : SoundCommand
	{
		[InjectArgument]
		BgmConfig m_Config = new BgmConfig();
		[InjectArgument]
		[InjectPathDefine("path", PathCategory.Bgm)]
		PlayConfig m_PlayConfig = PlayConfig.Bgm;

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed<BgmConfig>();
			if (m_PlayConfig.Loop)
			{
				data.Set(m_Config.Slot, new PlaySoundEnvData(m_Config.Group, m_PlayConfig));
			}
			else
			{
				data.Delete<PlaySoundEnvData>(m_Config.Slot);
			}
		}

		protected override void Preload(IPreLoader loader)
		{
			m_PlayConfig.Preload(Path.GetRoot(PathCategory.Bgm), loader);
		}

		protected override void Execute()
		{
			m_PlayConfig.Load(Path.GetRoot(PathCategory.Bgm), Cache);
			m_PlayHandle = Sound.PlayBgm(m_Config, m_PlayConfig);
		}

	}

	[TagName("bgm_volume")]
	public class BgmVolumeCommand : SoundCommand
	{
		[Argument]
		string m_Slot = "default";
		[InjectArgument]
		VolumeConfig m_Config = new VolumeConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed<BgmConfig>();
			data.Update<PlaySoundEnvData, VolumeConfig>(m_Slot, m_Config);
		}
		protected override void Execute()
		{
			m_PlayHandle = Sound.ChangeBgmVolume(m_Slot, m_Config);
		}
	}

	[TagName("bgm_stop")]
	public class BgmStopCommand : SoundCommand
	{
		[Argument]
		string m_Slot = "default";
		[InjectArgument]
		StopConfig m_Config = new StopConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed<BgmConfig>();
			data.Delete<PlaySoundEnvData>(m_Slot);
		}

		protected override void Execute()
		{
			m_PlayHandle = Sound.StopBgm(m_Slot, m_Config);
		}

	}

}