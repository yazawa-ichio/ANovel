using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ANovel.Minimum
{
	public class BgmConfig
	{
		public string Slot = "default";
		public float Volume = 1f;
		public float Time = 0.5f;
		public bool CrossFade = true;
		[NonSerialized]
		public AudioClip Clip;
	}

	public class SeConfig
	{
		public string Slot;
		public float Volume = 1f;
		public float Time = 0;
		public float PrevFadeTime = 0;
		[NonSerialized]
		public AudioClip Clip;
	}

	public interface ISoundController : IController
	{
		void PlaySe(SeConfig config);
		void StopSe(string slot, float time);
		void PlayBgm(BgmConfig config);
		void StopBgm(string slot, float time);
	}

	public class SoundController : ControllerBase, ISoundController
	{

		public override Type ControllerType => typeof(ISoundController);

		Queue<AudioSource> m_Pool = new Queue<AudioSource>();
		Dictionary<string, AudioSource> m_Bgm = new Dictionary<string, AudioSource>();
		Dictionary<string, AudioSource> m_SeSlot = new Dictionary<string, AudioSource>();
		List<string> m_PlaySlotList = new List<string>();
		List<AudioSource> m_NoneSlotList = new List<AudioSource>();

		AudioSource Get()
		{
			if (m_Pool.Count > 0)
			{
				var source = m_Pool.Dequeue();
				source.gameObject.SetActive(true);
				return source;
			}
			else
			{
				var source = new GameObject("SoundObject").AddComponent<AudioSource>();
				source.transform.SetParent(transform);
				return source;
			}
		}

		void Return(AudioSource source)
		{
			source.Stop();
			source.loop = false;
			source.gameObject.SetActive(false);
			source.clip = null;
			m_Pool.Enqueue(source);
		}

		public void PlayBgm(BgmConfig config)
		{
			if (m_Bgm.TryGetValue(config.Slot, out var source))
			{
				if (config.CrossFade && config.Time > 0)
				{
					StartCoroutine(FadeOut(source, config.Time, true));
				}
				else
				{
					Return(source);
				}
			}
			source = Get();
			source.loop = true;
			m_Bgm[config.Slot] = source;
			source.clip = config.Clip;
			if (config.Time > 0)
			{
				StartCoroutine(FadeIn(source, config.Volume, config.Time, true));
			}
			else
			{
				source.volume = config.Volume;
				source.Play();
			}
		}

		public void StopBgm(string slot, float time)
		{
			if (m_Bgm.TryGetValue(slot, out var source))
			{
				m_Bgm.Remove(slot);
				if (time > 0)
				{
					StartCoroutine(FadeOut(source, time, true));
				}
				else
				{
					Return(source);
				}
			}
		}

		public void PlaySe(SeConfig config)
		{
			if (!string.IsNullOrEmpty(config.Slot) && m_SeSlot.TryGetValue(config.Slot, out var source))
			{
				if (config.PrevFadeTime > 0)
				{
					StartCoroutine(FadeOut(source, config.PrevFadeTime, false));
				}
				else
				{
					Return(source);
				}
			}
			source = Get();
			source.loop = false;
			if (string.IsNullOrEmpty(config.Slot))
			{
				m_NoneSlotList.Add(source);
			}
			else
			{
				m_SeSlot[config.Slot] = source;
				m_PlaySlotList.Add(config.Slot);
			}
			source.clip = config.Clip;
			if (config.Time > 0)
			{
				StartCoroutine(FadeIn(source, config.Volume, config.Time, false));
			}
			else
			{
				source.volume = config.Volume;
				source.Play();
			}
		}

		public void StopSe(string slot, float time)
		{
			if (m_SeSlot.TryGetValue(slot, out var source))
			{
				m_SeSlot.Remove(slot);
				m_PlaySlotList.Remove(slot);
				if (time > 0)
				{
					StartCoroutine(FadeOut(source, time, false));
				}
				else
				{
					Return(source);
				}
			}
		}

		void Update()
		{
			if (IsPause)
			{
				return;
			}
			for (int i = m_NoneSlotList.Count - 1; i >= 0; i--)
			{
				var source = m_NoneSlotList[i];
				if (!source.isPlaying)
				{
					m_NoneSlotList.RemoveAt(i);
					Return(source);
				}
			}
			for (int i = m_PlaySlotList.Count - 1; i >= 0; i--)
			{
				var key = m_PlaySlotList[i];
				if (m_SeSlot.TryGetValue(key, out var source))
				{
					if (!source.isPlaying)
					{
						m_PlaySlotList.RemoveAt(i);
						m_SeSlot.Remove(key);
						Return(source);
					}
				}
				else
				{
					m_PlaySlotList.RemoveAt(i);
				}
			}
		}

		IEnumerator FadeOut(AudioSource source, float fadeTime, bool ignorePause)
		{
			float time = 0;
			float volume = source.volume;
			while (time < fadeTime)
			{
				time += ignorePause ? Time.deltaTime : DeltaTime;
				source.volume = (1f - time / fadeTime) * volume;
				yield return null;
			}
			Return(source);
		}

		IEnumerator FadeIn(AudioSource source, float volume, float fadeTime, bool ignorePause)
		{
			float time = 0;
			source.volume = 0;
			source.Play();
			while (time < fadeTime)
			{
				time += ignorePause ? Time.deltaTime : DeltaTime;
				source.volume = (time / fadeTime) * volume;
				yield return null;
			}
			source.volume = volume;
		}


		public override void Clear()
		{
			foreach (var key in m_Bgm.Keys.ToArray())
			{
				StopBgm(key, 0.2f);
			}
		}
		protected override void OnPause()
		{

			foreach (var source in GetComponentsInChildren<AudioSource>())
			{
				if (!m_Bgm.ContainsValue(source) && !source.loop)
				{
					source.Pause();
				}
			}
		}

		protected override void OnResume()
		{
			foreach (var source in GetComponentsInChildren<AudioSource>())
			{
				source.UnPause();
			}
		}

	}

}