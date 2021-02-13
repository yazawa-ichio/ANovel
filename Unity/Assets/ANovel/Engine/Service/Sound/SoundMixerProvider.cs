using UnityEngine;
using UnityEngine.Audio;

namespace ANovel.Service.Sound
{
	public interface IAudioMixerProvider
	{
		AudioMixerGroup GetGroup(SoundPlayerType type, string group);
		void Transition(string name, float time);
	}

	public class AudioMixerProvider : MonoBehaviour, IAudioMixerProvider
	{

		[SerializeField]
		AudioMixer m_Mixer = default;
		[SerializeField]
		AudioMixerGroup m_Bgm = default;
		[SerializeField]
		AudioMixerGroup m_Se = default;
		[SerializeField]
		AudioMixerGroup m_Voice = default;

		public AudioMixerGroup GetGroup(SoundPlayerType type, string group)
		{
			switch (type)
			{
				case SoundPlayerType.Bgm:
					return m_Bgm;
				case SoundPlayerType.Se:
					return m_Se;
				case SoundPlayerType.Voice:
					return m_Voice;
			}
			return null;
		}

		public void Transition(string name, float time)
		{
			var target = m_Mixer.FindSnapshot(name);
			m_Mixer.TransitionToSnapshots(new AudioMixerSnapshot[] { target }, new float[] { 1f }, time);
		}

	}


}