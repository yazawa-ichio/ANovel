using ANovel.Core;

namespace ANovel.Engine
{

	[TagName("define_autovoice")]
	public class DefineAutoVoiceCommand : PreProcess
	{
		[InjectArgument]
		AutoVoiceMetaData m_Param = new AutoVoiceMetaData();

		public override void Result(PreProcessor.Result result)
		{
			result.Meta.SetSingle(m_Param);
		}

	}

	[TagName("define_sound")]
	public class DefineSoundCommand : PreProcess, IImportText
	{
		[Argument]
		string m_Import = default;
		SoundMetaData m_Param;

		bool IImportText.Enabled => !string.IsNullOrEmpty(m_Import);

		string IImportText.Path => m_Import;

		void IImportText.Import(string text)
		{
			m_Param = UnityEngine.JsonUtility.FromJson<SoundMetaData>(text);
		}

		public override void Result(PreProcessor.Result result)
		{
			var data = SoundMetaData.Get(result.Meta);
			data.SE.InsertRange(0, m_Param.SE);
			data.BGM.InsertRange(0, m_Param.BGM);
		}

	}

	[TagName("define_se")]
	public class DefineSeCommand : PreProcess
	{
		[Argument]
		SeMetaData m_Param;

		public override void Result(PreProcessor.Result result)
		{
			var data = SoundMetaData.Get(result.Meta);
			data.SE.Insert(0, m_Param);
		}
	}

	[TagName("define_bgm")]
	public class DefineBgmCommand : PreProcess
	{
		[Argument]
		BgmMetaData m_Param;

		public override void Result(PreProcessor.Result result)
		{
			var data = SoundMetaData.Get(result.Meta);
			data.BGM.Insert(0, m_Param);
		}
	}

}