using ANovel.Core;

namespace ANovel.Engine
{

	[TagName("define_autovoice")]
	public class DefineAutoVoiceCommand : PreProcess
	{
		[InjectArgument]
		AutoVoiceMetaData m_Param = new AutoVoiceMetaData();

		public override void Result(PreProcessResult result)
		{
			result.Meta.SetSingle(m_Param);
		}

	}

#if ANOVEL_DEFINE_IMPORT

	[TagName("import_sound")]
	public class ImportSoundCommand : PreProcess, IImportText
	{
		[Argument(Required = true)]
		string m_Path = default;
		SoundMetaData m_Param;

		string IImportText.Path => m_Path;

		void IImportText.Import(string text)
		{
			m_Param = UnityEngine.JsonUtility.FromJson<SoundMetaData>(text);
		}

		public override void Result(PreProcessResult result)
		{
			var data = SoundMetaData.Get(result.Meta);
			data.SE.InsertRange(0, m_Param.SE);
			data.BGM.InsertRange(0, m_Param.BGM);
			if (!result.Converters.Contains(data))
			{
				result.Converters.Add(data);
			}
		}

	}

#endif

	[TagName("define_se")]
	[ReplaceTagDefine("@se_{name}", "@se path={path}")]
	public class DefineSeCommand : PreProcess
	{
		[Argument]
		SeMetaData m_Param;

		public override void Result(PreProcessResult result)
		{
			var data = SoundMetaData.Get(result.Meta);
			data.SE.Insert(0, m_Param);
			if (!result.Converters.Contains(data))
			{
				result.Converters.Add(data);
			}
		}
	}

	[TagName("define_bgm")]
	[ReplaceTagDefine("@bgm_{name}", "@se path={path}")]
	public class DefineBgmCommand : PreProcess
	{
		[Argument]
		BgmMetaData m_Param;

		public override void Result(PreProcessResult result)
		{
			var data = SoundMetaData.Get(result.Meta);
			data.BGM.Insert(0, m_Param);
			if (!result.Converters.Contains(data))
			{
				result.Converters.Add(data);
			}
		}
	}

}