using ANovel.Engine.PostEffects;

namespace ANovel.Engine
{
	public abstract class PSCommandBase<T> : Command, IUseTransitionScope where T : struct, IPostEffectParam
	{
		protected PostEffectService Service => Get<PostEffectService>();

		[InjectArgument]
		protected T m_Param;

		bool IUseTransitionScope.Use => true;

		public PSCommandBase()
		{
			m_Param = GetDefault();
		}

		protected override void UpdateEnvData(IEnvData data)
		{
			data.SetSingle(m_Param);
		}

		protected abstract T GetDefault();

		protected override void Execute()
		{
			Service.Run(m_Param);
		}

	}

	[TagName("ps_blur")]
	public class PSBlurCommand : PSCommandBase<BlurParam>
	{
		protected override BlurParam GetDefault()
		{
			return BlurParam.Default;
		}
	}

	[TagName("ps_bloom")]
	public class PSBloomCommand : PSCommandBase<BloomParam>
	{
		protected override BloomParam GetDefault()
		{
			return BloomParam.Default;
		}
	}

	[TagName("ps_color")]
	public class PSSimpleColorEffectCommand : PSCommandBase<SimpleColorEffectParam>
	{
		protected override SimpleColorEffectParam GetDefault()
		{
			return SimpleColorEffectParam.Default;
		}
	}

	[TagName("ps_hsv_shift")]
	public class PSHsvShiftParamCommand : PSCommandBase<HsvShiftParam>
	{
		protected override HsvShiftParam GetDefault()
		{
			return HsvShiftParam.Default;
		}
	}

	[TagName("ps_clear")]
	public class PSClearCommand : Command, IUseTransitionScope
	{
		protected PostEffectService Service => Get<PostEffectService>();

		bool IUseTransitionScope.Use => true;

		protected override void UpdateEnvData(IEnvData data)
		{
			data.DeleteAllByInterface<IPostEffectParam>();
		}

		protected override void Execute()
		{
			Service.Clear();
		}
	}
}