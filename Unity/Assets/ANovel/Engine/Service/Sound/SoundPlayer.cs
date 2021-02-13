using ANovel.Core;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace ANovel.Service.Sound
{
	public class SoundPlayer : IDisposable
	{
		AudioSource m_Source;
		ComponentPool<AudioSource> m_Pool;
		FloatFadeHandle m_VolumeHandle = default;
		ICacheHandle<AudioClip> m_Cache;
		Action<float> m_SetVolume;
		float m_PauseVolume;
		bool m_IsStop;
		bool m_IsPause;

		public bool IsValid => m_Source != null;

		public FloatFadeHandle Play(ComponentPool<AudioSource> pool, PlayConfig config, AudioMixerGroup mixerGroup)
		{
			m_Pool = pool;
			m_Cache = config.Clip;
			m_Source = m_Pool.Get();
			m_Source.clip = m_Cache.Value;
			m_Source.volume = 0;
			m_Source.loop = config.Loop;
			m_Source.pitch = config.Pitch;
			m_Source.panStereo = config.Pan;
			m_Source.outputAudioMixerGroup = mixerGroup;
			m_SetVolume = SetVolume;
			m_Source.Play();
			return Volume(config.Volume, config.Time, config.Easing.GetMethodOrNull());
		}

		void SetVolume(float val)
		{
			if (m_Source != null)
			{
				m_Source.volume = val;
			}
		}

		public FloatFadeHandle Volume(float value, Millisecond time, Func<float, float> easing)
		{
			if (m_IsStop)
			{
				return FloatFadeHandle.Empty;
			}
			m_VolumeHandle?.Dispose();
			if (time.Value <= 0)
			{
				m_Source.volume = value;
				return m_VolumeHandle = FloatFadeHandle.Empty;
			}
			return m_VolumeHandle = new FloatFadeHandle(value, m_Source.volume, time.ToSecond())
			{
				Output = m_SetVolume,
				Easing = easing,
			};
		}

		public FloatFadeHandle Volume(VolumeConfig config)
		{
			return Volume(config.Volume, config.Time, config.Easing.GetMethodOrNull());
		}

		public FloatFadeHandle Stop(Millisecond time, Func<float, float> easing)
		{
			if (m_IsPause && !m_Source.isPlaying)
			{
				m_IsStop = true;
				Dispose();
				return FloatFadeHandle.Empty;
			}
			var handle = Volume(0, time, easing);
			m_IsStop = true;
			handle.OnComplete += Dispose;
			return handle;
		}

		public FloatFadeHandle Stop(StopConfig config)
		{
			if (m_IsPause && !m_Source.isPlaying)
			{
				m_IsStop = true;
				Dispose();
				return FloatFadeHandle.Empty;
			}
			var handle = Volume(0, config.Time, config.Easing.GetMethodOrNull());
			m_IsStop = true;
			handle.OnComplete += Dispose;
			return handle;
		}

		public void Update(float deltaTime)
		{
			if (m_Source == null)
			{
				return;
			}
			m_VolumeHandle?.Update(deltaTime);
			if (m_IsStop && m_VolumeHandle == null)
			{
				Dispose();
				return;
			}
			if (m_Source == null)
			{
				Dispose();
				return;
			}
			if (!m_IsPause && !m_Source.isPlaying)
			{
				Dispose();
				return;
			}
		}

		public void Dispose()
		{
			if (m_Source == null)
			{
				return;
			}
			m_Source.transform.localPosition = Vector3.zero;
			m_Source.loop = false;
			m_Source.clip = null;
			m_Cache = null;
			m_Pool.Return(m_Source);
			m_Source = null;
		}

		public FloatFadeHandle Pause(Millisecond time)
		{
			m_IsPause = true;
			m_VolumeHandle?.Dispose();
			m_PauseVolume = m_Source.volume;
			var handle = Volume(0, time, null);
			handle.OnComplete += () =>
			{
				if (m_Source != null)
				{
					m_Source.Pause();
				}
			};
			return handle;
		}

		public FloatFadeHandle Resume(Millisecond time)
		{
			m_IsPause = false;
			m_VolumeHandle?.Dispose();
			m_Source.UnPause();
			return Volume(m_PauseVolume, time, null);
		}

		public void SystemPause()
		{
			if (m_Source != null)
			{
				m_Source.Pause();
			}
		}

		public void SystemResume()
		{
			if (m_Source != null && !m_IsPause)
			{
				m_Source.UnPause();
			}
		}

	}


}