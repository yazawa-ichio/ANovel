using ANovel.Actions;
using Category = ANovel.Engine.ImageService.Category;

namespace ANovel.Engine
{

	public abstract class ImageObjectActionCommandBase : Command
	{

		public abstract string TargetName { get; }

		protected abstract Category Category { get; }

		ImageService Service => Get<ImageService>();

		[Argument(KeyName = "param", Required = true)]
		string m_Param;
		[Argument]
		bool m_Cont;
		[Argument]
		[Description("実行を同期するか？")]
		protected bool m_Sync = false;
		[Argument]
		[Description("スキップ可能か？")]
		protected bool m_CanSkip = true;

		bool m_Skip;
		ImageActionEnvData m_Data;
		ActionPlayingHandle m_PlayHandle;

		public override bool IsEnd() => !m_PlayHandle?.IsPlaying ?? true;

		public override bool IsSync() => m_Sync;

		protected override void TryNext()
		{
			if (m_Sync && m_CanSkip)
			{
				m_PlayHandle?.Dispose();
			}
		}

		protected override void UpdateEnvData(IEnvData data)
		{
			data = data.Prefixed(Category);
			data.Delete<ImageActionEnvData>(TargetName);
			data.Delete<ActionTimeEnvData>(TargetName);
			if (!data.Has<ImageObjectEnvData>(TargetName))
			{
				m_Skip = true;
				return;
			}
			m_Data = new ImageActionEnvData
			{
				Value = ActionMetaData.CreateData(m_Param, this, data),
			};
			if (!m_Sync && m_Cont)
			{
				data.Set(TargetName, m_Data);
				data.Set(TargetName, new ActionTimeEnvData());
			}
		}

		protected override void Execute()
		{
			if (m_Skip) return;
			var handle = Service.PlayAction(Category, TargetName, ref m_Data);
			if (m_Sync || !m_Cont)
			{
				m_PlayHandle = handle;
			}
		}

	}


	[TagName("bg_action")]
	public class ActionBgCommand : ImageObjectActionCommandBase
	{
		public override string TargetName => BGCommandBase.EnvKey;

		protected override Category Category => Category.Bg;
	}

	[TagName("chara_action")]
	public class ActionCharaCommand : ImageObjectActionCommandBase
	{
		[Argument(Required = true)]
		string m_Name = null;

		public override string TargetName => CharaMetaData.GetKey(Meta, m_Name);

		protected override Category Category => Category.Chara;
	}

	[TagName("image_action")]
	public class ActionImageCommand : ImageObjectActionCommandBase
	{
		[Argument(Required = true)]
		string m_Name = null;

		public override string TargetName => m_Name;

		protected override Category Category => Category.Image;

	}
}


