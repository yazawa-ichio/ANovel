using ANovel.Core;
using UnityEngine;

namespace ANovel.Minimum
{
	[CommandName("se", Symbol = "ANOVEL_MINIMUM")]
	public class SeCommand : MinimumCommand
	{
		[CommandField(Required = true)]
		string m_Path = default;
		[InjectParam]
		SeConfig m_Config = new SeConfig();
		[CommandField]
		float m_Delay = default;
		[CommandField]
		bool m_Sync = default;
		bool m_Play;
		float m_Timer;

		public override bool IsEnd()
		{
			return m_Play;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void Preload(IPreLoader loader)
		{
			loader.Load<AudioClip>(Config.GetSePath(m_Path));
		}

		protected override void Execute()
		{
			m_Timer = 0;
			if (m_Delay <= 0)
			{
				Play();
			}
		}

		protected override void Update()
		{
			if (!m_Play)
			{
				m_Timer += Sound.DeltaTime;
				if (m_Timer >= m_Delay)
				{
					Play();
				}
			}
		}

		void Play()
		{
			m_Play = true;
			m_Config.Clip = Cache.Get<AudioClip>(Config.GetSePath(m_Path));
			Sound.PlaySe(m_Config);
		}

	}

	[CommandName("stop_se", Symbol = "ANOVEL_MINIMUM")]
	public class StopSeCommand : MinimumCommand
	{
		[CommandField(Required = true)]
		string m_Slot = null;
		[CommandField]
		float m_Time = 0;

		protected override void Execute()
		{
			Sound.StopSe(m_Slot, m_Time);
		}
	}

	[CommandName("bgm", Symbol = "ANOVEL_MINIMUM")]
	public class BgmCommand : MinimumCommand
	{
		[CommandField(Required = true)]
		string m_Path = default;
		[InjectParam]
		BgmConfig m_Config = new BgmConfig();

		protected override void Preload(IPreLoader loader)
		{
			loader.Load<AudioClip>(Config.GetBgmPath(m_Path));
		}

		protected override void Execute()
		{
			m_Config.Clip = Cache.Get<AudioClip>(Config.GetBgmPath(m_Path));
			Sound.PlayBgm(m_Config);
		}

	}

	[CommandName("stop_bgm", Symbol = "ANOVEL_MINIMUM")]
	public class StopBgmCommand : MinimumCommand
	{
		[CommandField]
		string m_Slot = "default";
		[CommandField]
		float m_Time = 0.5f;

		protected override void Execute()
		{
			Sound.StopBgm(m_Slot, m_Time);
		}
	}

}