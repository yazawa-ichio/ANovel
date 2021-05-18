using ANovel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace ANovel.Service.Sound
{
	public interface ISoundService
	{
		IPlayHandle PlayBgm(BgmConfig config, PlayConfig playConfig);
		IPlayHandle StopBgm(string slot, StopConfig config);
		IPlayHandle ChangeBgmVolume(string slot, VolumeConfig config);
		IPlayHandle PlaySe(SeConfig config, PlayConfig playConfig);
		IPlayHandle StopSe(string slot, StopConfig config);
		IPlayHandle StopAllSe(StopConfig config);
		IPlayHandle ChangeSeVolume(string slot, VolumeConfig config);

		IPlayHandle PlayVoice(VoiceConfig config, PlayConfig playConfig);
		IPlayHandle StopVoice(string slot, StopConfig config);
		IPlayHandle StopAllVoice(StopConfig config);


	}

	public interface IReplayVoiceService
	{
		Task ReplayVoice(IHistoryLog log);
	}

	public class SoundService : Service, ISoundService, IReplayVoiceService
	{
		public override Type ServiceType => typeof(ISoundService);

		public IAudioMixerProvider Mixer { get; private set; }

		ComponentPool<AudioSource> m_Pool;
		Dictionary<string, SoundPlayer> m_Bgm = new Dictionary<string, SoundPlayer>();
		Dictionary<string, SoundPlayer> m_Se = new Dictionary<string, SoundPlayer>();
		Dictionary<string, SoundPlayer> m_Voice = new Dictionary<string, SoundPlayer>();
		List<SoundPlayer> m_Playing = new List<SoundPlayer>();
		Transform m_Root;

		protected override void Initialize()
		{
			Container.Set<IReplayVoiceService>(this);
			var obj = new GameObject(typeof(SoundService).Name);
			m_Root = obj.transform;
			m_Root.SetParent(transform);

			m_Pool = new ComponentPool<AudioSource>(m_Root);
			Mixer = GetComponent<IAudioMixerProvider>();
			Event.Register(this);
		}


		protected override void OnUpdate(IEngineTime time)
		{
			UpdatePlaying(time);
		}

		void UpdatePlaying(IEngineTime time)
		{
			var deltaTime = time.DeltaTime;
			for (int i = m_Playing.Count - 1; i >= 0; i--)
			{
				var player = m_Playing[i];
				if (player.IsValid)
				{
					player.Update(deltaTime);
				}
				if (!player.IsValid)
				{
					m_Playing.RemoveAt(i);
				}
			}
		}

		AudioMixerGroup GetMixerGroup(SoundPlayerType type, string group)
		{
			if (Mixer != null)
			{
				return Mixer.GetGroup(type, group);
			}
			return null;
		}

		public IPlayHandle PlayBgm(BgmConfig config, PlayConfig playConfig)
		{
			FloatFadeHandle prev = null;
			if (m_Bgm.TryGetValue(config.Slot, out var player))
			{
				prev = player.Stop(playConfig.Time, config.StopEasing.GetMethodOrNull());
				if (!config.CrossFade)
				{
					prev.Dispose();
					prev = null;
				}
			}
			m_Bgm[config.Slot] = player = new SoundPlayer();
			m_Playing.Add(player);
			var handle = player.Play(m_Pool, playConfig, GetMixerGroup(SoundPlayerType.Bgm, config.Group));
			if (prev != null)
			{
				handle.OnComplete += prev.Dispose;
			}
			return handle;
		}

		public IPlayHandle StopBgm(string slot, StopConfig config)
		{
			if (m_Bgm.TryGetValue(slot, out var player))
			{
				return player.Stop(config);
			}
			return FloatFadeHandle.Empty;
		}

		public IPlayHandle PlaySe(SeConfig config, PlayConfig playConfig)
		{
			if (!string.IsNullOrEmpty(config.Slot) && m_Se.TryGetValue(config.Slot, out var player))
			{
				player.Stop(Millisecond.FromSecond(0.1f), null);
			}
			player = new SoundPlayer();
			m_Playing.Add(player);
			if (string.IsNullOrEmpty(config.Slot))
			{
				Debug.Assert(!playConfig.Loop);
				playConfig.Loop = false;
			}
			else
			{
				m_Se[config.Slot] = player;
			}
			return player.Play(m_Pool, playConfig, GetMixerGroup(SoundPlayerType.Se, config.Group));
		}

		public IPlayHandle StopSe(string slot, StopConfig config)
		{
			if (m_Se.TryGetValue(slot, out var player))
			{
				return player.Stop(config);
			}
			return FloatFadeHandle.Empty;
		}

		public IPlayHandle StopAllSe(StopConfig config)
		{
			var handles = m_Playing.Select(x => x.Stop(config) as IPlayHandle).ToArray();
			return new CombinePlayHandle(handles);
		}

		public IPlayHandle PlayVoice(VoiceConfig config, PlayConfig playConfig)
		{
			if (m_Voice.TryGetValue(config.Slot, out var player))
			{
				player.Dispose();
			}
			m_Voice[config.Slot] = player = new SoundPlayer();
			m_Playing.Add(player);
			return player.Play(m_Pool, playConfig, GetMixerGroup(SoundPlayerType.Voice, config.Group));
		}

		public IPlayHandle StopVoice(string slot, StopConfig config)
		{
			if (m_Voice.TryGetValue(slot, out var player))
			{
				m_Voice.Remove(slot);
				return player.Stop(config);
			}
			return FloatFadeHandle.Empty;
		}

		public IPlayHandle StopAllVoice(StopConfig config)
		{
			var handles = m_Voice.Values.Select(x => x.Stop(config) as IPlayHandle).ToArray();
			m_Voice.Clear();
			return new CombinePlayHandle(handles);
		}

		public IPlayHandle ChangeBgmVolume(string slot, VolumeConfig config)
		{
			if (m_Bgm.TryGetValue(slot, out var player))
			{
				return player.Volume(config);
			}
			return FloatFadeHandle.Empty;
		}


		public IPlayHandle ChangeSeVolume(string slot, VolumeConfig config)
		{
			if (m_Se.TryGetValue(slot, out var player))
			{
				return player.Volume(config);
			}
			return FloatFadeHandle.Empty;
		}

		public void StopAll()
		{
			foreach (var bgm in m_Bgm.Values)
			{
				bgm.Dispose();
			}
			m_Bgm.Clear();
			foreach (var se in m_Se.Values)
			{
				se.Dispose();
			}
			m_Se.Clear();
			foreach (var voice in m_Voice.Values)
			{
				voice.Dispose();
			}
			m_Voice.Clear();
			foreach (var se in m_Playing)
			{
				se.Dispose();
			}
			m_Playing.Clear();
		}

		protected override void PreRestore(IMetaData meta, IEnvDataHolder data, IPreLoader loader)
		{
			foreach (var kvp in PrefixedEnvData.Get<BgmConfig>(data).GetAll<PlaySoundEnvData>())
			{
				loader.Load<AudioClip>(Path.GetBgm(kvp.Value.Path));
			}
			foreach (var kvp in PrefixedEnvData.Get<SeConfig>(data).GetAll<PlaySoundEnvData>())
			{
				loader.Load<AudioClip>(Path.GetSe(kvp.Value.Path));
			}
		}

		protected override void Restore(IMetaData meta, IEnvDataHolder data, ResourceCache cache)
		{
			StopAll();
			foreach (var kvp in PrefixedEnvData.Get<BgmConfig>(data).GetAll<PlaySoundEnvData>())
			{
				var config = PlayConfig.Restore(kvp.Value, Path.BgmRoot, cache);
				PlayBgm(new BgmConfig { Slot = kvp.Key, Group = kvp.Value.Group }, config);
			}
			foreach (var kvp in PrefixedEnvData.Get<SeConfig>(data).GetAll<PlaySoundEnvData>())
			{
				var config = PlayConfig.Restore(kvp.Value, Path.SeRoot, cache);
				PlaySe(new SeConfig { Slot = kvp.Key, Group = kvp.Value.Group }, config);
			}
		}

		public async Task ReplayVoice(IHistoryLog log)
		{
			var cache = Container.Get<ResourceCache>();
			var configs = new Dictionary<string, PlayConfig>();
			foreach (var kvp in log.Extension.GetAll<PlayVoiceEnvData>())
			{
				configs.Add(kvp.Key, PlayConfig.Restore(kvp.Value, Path.VoiceRoot, cache));
			}
			await Task.WhenAll(configs.Values.Select(x => x.Clip.GetAsync()));
			foreach (var kvp in configs)
			{
				var voice = log.Extension.Get<PlayVoiceEnvData>(kvp.Key);
				PlayVoice(new VoiceConfig { Slot = kvp.Key, Group = voice.Group }, kvp.Value);
			}
		}

	}


}