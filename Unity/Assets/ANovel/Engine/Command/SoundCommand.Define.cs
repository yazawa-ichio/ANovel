using ANovel.Core;
using ANovel.Service;

namespace ANovel.Commands
{

	[PreProcessName("define_autovoice")]
	public class DefineAutoVoiceCommand : PreProcess
	{
		[InjectParam]
		AutoVoiceMetaData m_Param = new AutoVoiceMetaData();

		public override void Result(PreProcessor.Result result)
		{
			result.Meta.SetSingle(m_Param);
		}

	}

	[PreProcessName("define_sound")]
	public class DefineSoundCommand : PreProcess, IImportText
	{
		[CommandField]
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

	[PreProcessName("define_se")]
	public class DefineSeCommand : PreProcess
	{
		[CommandField]
		SeMetaData m_Param;

		public override void Result(PreProcessor.Result result)
		{
			var data = SoundMetaData.Get(result.Meta);
			data.SE.Insert(0, m_Param);
		}
	}

	[PreProcessName("define_bgm")]
	public class DefineBgmCommand : PreProcess
	{
		[CommandField]
		BgmMetaData m_Param;

		public override void Result(PreProcessor.Result result)
		{
			var data = SoundMetaData.Get(result.Meta);
			data.BGM.Insert(0, m_Param);
		}
	}

}