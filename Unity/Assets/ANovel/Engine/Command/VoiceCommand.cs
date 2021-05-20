namespace ANovel.Engine
{


	[TagName("voice")]
	public class VoicePlayCommand : SoundCommand
	{
		public static VoicePlayCommand Create(string path)
		{
			var cmd = new VoicePlayCommand();
			cmd.m_PlayConfig.Path = path;
			return cmd;
		}

		[InjectArgument]
		VoiceConfig m_Config = new VoiceConfig();
		[InjectArgument(IgnoreKey = nameof(PlayConfig.Loop))]
		PlayConfig m_PlayConfig = PlayConfig.Voice;

		protected override void UpdateEnvData(IEnvData data)
		{
			data.Set(m_Config.Slot, new PlayVoiceEnvData(m_Config.Group, m_PlayConfig));
		}

		protected override void Preload(IPreLoader loader)
		{
			m_PlayConfig.Preload(Path.VoiceRoot, loader);
		}

		protected override void Execute()
		{
			m_PlayConfig.Load(Path.VoiceRoot, Cache);
			m_PlayHandle = Sound.PlayVoice(m_Config, m_PlayConfig);
		}

	}

	[TagName("voice_stop")]
	public class VoiceStopCommand : SoundCommand
	{
		[Argument]
		string m_Slot = "default";
		[InjectArgument]
		StopConfig m_Config = new StopConfig();

		protected override void Execute()
		{
			m_PlayHandle = Sound.StopVoice(m_Slot, m_Config);
		}
	}

	[TagName("voice_stop_all")]
	public class VoiceStopAllCommand : SoundCommand
	{
		[InjectArgument]
		StopConfig m_Config = new StopConfig();

		protected override void Execute()
		{
			m_PlayHandle = Sound.StopAllVoice(m_Config);
		}
	}

	[TagName("autovoice")]
	public class AutoVoiceCommand : SoundCommand
	{
		[Argument]
		bool m_Enabled = true;
		[Argument]
		int? m_Index;

		protected override void UpdateEnvData(IEnvData data)
		{
			data.TryGetSingle<AutoVoiceEnvData>(out var autovoice);
			autovoice.Enabled = m_Enabled;
			autovoice.Index = m_Index.GetValueOrDefault(autovoice.Index);
			data.SetSingle(autovoice);
		}

	}

	[TagName("autovoice_reset")]
	public class AutoVoiceResetCommand : SoundCommand
	{
		[Argument]
		bool m_Common = true;
		[Argument]
		bool m_Chara = true;

		protected override void UpdateEnvData(IEnvData data)
		{
			if (m_Common)
			{
				if (data.TryGetSingle<AutoVoiceEnvData>(out var autovoice))
				{
					autovoice.Index = 0;
					data.SetSingle(autovoice);
				}
			}
			if (m_Chara)
			{
				foreach (var kvp in data.GetAll<CharaAutoVoiceEnvData>())
				{
					var value = kvp.Value;
					value.Index = 0;
					data.Set(kvp.Key, value);
				}
			}
		}
	}


	[TagName("autovoice_skip")]
	public class SkipAutoVoiceCommand : SoundCommand
	{
		protected override void UpdateEnvData(IEnvData data)
		{
			data.SetSingle(new SkipAutoVoiceEnvData());
		}
	}


}