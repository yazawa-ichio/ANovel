using ANovel.Core;
using UnityEngine;

namespace ANovel.Minimum
{
	public enum Level
	{
		BG,
		Back,
		Middle,
		Front,
		Over,
	}


	[CommandName("show", Symbol = "ANOVEL_MINIMUM")]
	public class ShowImageCommand : MinimumCommand
	{
		[CommandField(Required = true)]
		string m_Path = default;
		[InjectParam]
		ShowConfig m_Config = new ShowConfig();
		[InjectParam]
		LayoutConfig m_Layout = new LayoutConfig();
		[CommandField]
		bool m_Sync = default;
		[CommandField]
		bool m_Skip = true;

		CommandCoroutine m_Command;

		public override bool IsEnd()
		{
			return m_Command.IsEnd;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void Preload(IPreLoader loader)
		{
			loader.Load<Sprite>(Config.GetImagePath(m_Path));
		}

		protected override void Execute()
		{
			m_Config.Sprite = Cache.Get<Sprite>(Config.GetImagePath(m_Path));
			m_Command = Image.ShowImage(m_Config, m_Layout).Run();
		}

		protected override void TryNext()
		{
			if (m_Sync && m_Skip)
			{
				m_Command?.Finish();
			}
		}

		public override void FinishBlock()
		{
			m_Command?.Finish();
		}

	}


	[CommandName("hide", Symbol = "ANOVEL_MINIMUM")]
	public class HideImageCommand : MinimumCommand
	{
		[InjectParam]
		HideConfig m_Config = new HideConfig();
		[CommandField]
		bool m_Sync = default;
		[CommandField]
		bool m_Skip = true;

		CommandCoroutine m_Command;

		public override bool IsEnd()
		{
			return m_Command.IsEnd;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void Execute()
		{
			m_Command = Image.HideImage(m_Config).Run();
		}

		protected override void TryNext()
		{
			if (m_Sync && m_Skip)
			{
				m_Command?.Finish();
			}
		}

		public override void FinishBlock()
		{
			m_Command?.Finish();
		}

	}

	[CommandName("all_hide", Symbol = "ANOVEL_MINIMUM")]
	public class AllHideImageCommand : MinimumCommand
	{
		[InjectParam]
		AllHideConfig m_Config = new AllHideConfig();
		[CommandField]
		bool m_Sync = default;
		[CommandField]
		bool m_Skip = true;

		CommandCoroutine m_Command;

		public override bool IsEnd()
		{
			return m_Command.IsEnd;
		}

		public override bool IsSync()
		{
			return m_Sync;
		}

		protected override void Execute()
		{
			m_Command = Image.AllHideImage(m_Config).Run();
		}

		protected override void TryNext()
		{
			if (m_Sync && m_Skip)
			{
				m_Command?.Finish();
			}
		}

		public override void FinishBlock()
		{
			m_Command?.Finish();
		}
	}

	[CommandName("layout", Symbol = "ANOVEL_MINIMUM")]
	public class LayoutImageCommand : MinimumCommand
	{
		[InjectParam]
		LayoutConfig m_Config = new LayoutConfig();

		protected override void Execute()
		{
			Image.LayoutImage(m_Config);
		}

	}

}