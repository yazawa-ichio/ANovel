using ANovel.Core;
using ANovel.Service.Sound;

namespace ANovel.Commands
{
	public abstract class SoundCommand : SyncCommandBase
	{
		protected ISoundService Sound => Get<ISoundService>();
	}

	[CommandName("se")]
	public class SePlayCommand : SoundCommand
	{
		[InjectParam]
		SeConfig m_Config = new SeConfig();
		[InjectParam]
		PlayConfig m_PlayConfig = PlayConfig.Se;

		protected override void UpdateEnvData(IEnvData data)
		{
			if (string.IsNullOrEmpty(m_Config.Slot))
			{
				return;
			}
			data = PrefixedEnvData.Get<SeConfig>(data);
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
			m_PlayConfig.Preload(Path.SeRoot, loader);
		}

		protected override void Execute()
		{
			m_PlayConfig.Load(Path.SeRoot, Cache);
			m_PlayHandle = Sound.PlaySe(m_Config, m_PlayConfig);
		}
	}

	[CommandName("se_stop")]
	public class SeStopCommand : SoundCommand
	{
		[CommandField(Required = true)]
		string m_Slot = null;
		[InjectParam]
		StopConfig m_Config = new StopConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			if (string.IsNullOrEmpty(m_Slot))
			{
				return;
			}
			data = PrefixedEnvData.Get<SeConfig>(data);
			data.Delete<PlaySoundEnvData>(m_Slot);
		}

		protected override void Execute()
		{
			m_PlayHandle = Sound.StopSe(m_Slot, m_Config);
		}
	}

	[CommandName("se_stop_all")]
	public class SeStopAllCommand : SoundCommand
	{
		[InjectParam]
		StopConfig m_Config = new StopConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = PrefixedEnvData.Get<SeConfig>(data);
			data.DeleteAll<PlaySoundEnvData>();
		}

		protected override void Execute()
		{
			m_PlayHandle = Sound.StopAllSe(m_Config);
		}
	}


	[CommandName("se_volume")]
	public class SeVolumeCommand : SoundCommand
	{
		[CommandField(Required = true)]
		string m_Slot = null;
		[InjectParam]
		VolumeConfig m_Config = new VolumeConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = PrefixedEnvData.Get<SeConfig>(data);
			data.Update<PlaySoundEnvData, VolumeConfig>(m_Slot, m_Config);
		}

		protected override void Execute()
		{
			m_PlayHandle = Sound.ChangeBgmVolume(m_Slot, m_Config);
		}

	}


	[CommandName("bgm")]
	public class BgmPlayCommand : SoundCommand
	{
		[InjectParam]
		BgmConfig m_Config = new BgmConfig();
		[InjectParam]
		PlayConfig m_PlayConfig = PlayConfig.Bgm;

		protected override void UpdateEnvData(IEnvData data)
		{
			data = PrefixedEnvData.Get<BgmConfig>(data);
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
			m_PlayConfig.Preload(Path.BgmRoot, loader);
		}

		protected override void Execute()
		{
			m_PlayConfig.Load(Path.BgmRoot, Cache);
			m_PlayHandle = Sound.PlayBgm(m_Config, m_PlayConfig);
		}

	}

	[CommandName("bgm_volume")]
	public class BgmVolumeCommand : SoundCommand
	{
		[CommandField]
		string m_Slot = "default";
		[InjectParam]
		VolumeConfig m_Config = new VolumeConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = PrefixedEnvData.Get<BgmConfig>(data);
			data.Update<PlaySoundEnvData, VolumeConfig>(m_Slot, m_Config);
		}
		protected override void Execute()
		{
			m_PlayHandle = Sound.ChangeBgmVolume(m_Slot, m_Config);
		}
	}

	[CommandName("bgm_stop")]
	public class BgmStopCommand : SoundCommand
	{
		[CommandField]
		string m_Slot = "default";
		[InjectParam]
		StopConfig m_Config = new StopConfig();

		protected override void UpdateEnvData(IEnvData data)
		{
			data = PrefixedEnvData.Get<BgmConfig>(data);
			data.Delete<PlaySoundEnvData>(m_Slot);
		}

		protected override void Execute()
		{
			m_PlayHandle = Sound.StopBgm(m_Slot, m_Config);
		}

	}

}