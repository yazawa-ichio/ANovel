﻿using UnityEngine;

namespace ANovel.Engine
{
	public class PlayConfig
	{
		public static PlayConfig Se => new PlayConfig
		{
		};
		public static PlayConfig Bgm => new PlayConfig
		{
			Time = Millisecond.FromSecond(0.5f),
			Loop = true,
		};
		public static PlayConfig Voice => new PlayConfig
		{
		};

		[Argument(Required = true)]
		public string Path;
		[RateArgument]
		public float Volume = 1f;
		public Millisecond Time;
		public bool Loop;
		public Easing? Easing;
		[RateArgument]
		public float Pitch = 1f;
		[RateArgument]
		public float Pan;
		[SkipArgument]
		public ICacheHandle<AudioClip> Clip { get; set; }

		public void Preload(string prefix, IPreLoader loader)
		{
			loader.Load<AudioClip>(prefix + Path);
		}

		public void Load(string prefix, IResourceCache cache)
		{
			Clip = cache.Load<AudioClip>(prefix + Path);
		}

		public static PlayConfig Restore(PlaySoundEnvData data, string prefix, IResourceCache cache)
		{
			return new PlayConfig()
			{
				Pitch = data.Pitch,
				Pan = data.Pan,
				Volume = data.Volume,
				Loop = true,
				Time = Millisecond.FromSecond(0.2f),
				Clip = cache.Load<AudioClip>(prefix + data.Path),
			};
		}

		public static PlayConfig Restore(PlayVoiceEnvData data, string prefix, IResourceCache cache)
		{
			return new PlayConfig()
			{
				Pitch = data.Pitch,
				Pan = data.Pan,
				Volume = data.Volume,
				Clip = cache.Load<AudioClip>(prefix + data.Path),
			};
		}
	}

	public class VolumeConfig
	{
		[Argument(Required = true)]
		[RateArgument]
		public float Volume = 1f;
		public Millisecond Time = Millisecond.FromSecond(0.5f);
		public Easing? Easing;
	}

	public class StopConfig
	{
		public Millisecond Time { get; set; }
		public Easing? Easing { get; set; }
	}

	public class SeConfig
	{
		public string Slot { get; set; }
		public string Group = "SE";
	}

	public class BgmConfig
	{
		public string Slot { get; set; } = "default";
		public string Group { get; set; } = "BGM";
		public bool CrossFade { get; set; } = true;
		public Easing? StopEasing { get; set; }
	}

	public class VoiceConfig
	{
		public string Slot { get; set; } = "default";
		public string Group = "Voice";
	}

	public struct PlaySoundEnvData : IEnvValue, IEnvValueUpdate<VolumeConfig>
	{
		public string Path;
		public string Group;
		public float Volume;
		public float Pitch;
		public float Pan;

		public PlaySoundEnvData(string group, PlayConfig config)
		{
			Path = config.Path;
			Group = group;
			Volume = config.Volume;
			Pitch = config.Pitch;
			Pan = config.Pan;
		}

		public void Update(VolumeConfig arg)
		{
			Volume = arg.Volume;
		}

	}

	public struct PlayVoiceEnvData : IEnvValue, IPreProcessDelete, IHistorySaveEnvData
	{
		public string Path;
		public string Group;
		public float Volume;
		public float Pitch;
		public float Pan;

		public PlayVoiceEnvData(string group, PlayConfig config)
		{
			Path = config.Path;
			Group = group;
			Volume = config.Volume;
			Pitch = config.Pitch;
			Pan = config.Pan;
		}

	}

	public struct AutoVoiceEnvData : IEnvValue
	{
		public bool Enabled;
		public int Index;
	}

	public struct CharaAutoVoiceEnvData : IEnvValue
	{
		public int Index;
	}

	public struct SkipAutoVoiceEnvData : IEnvValue, IPreProcessDelete
	{
	}


}