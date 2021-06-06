using ANovel.Core;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Engine
{
	public class SoundMetaData : IParamConverter
	{
		public static SoundMetaData Get(MetaData meta)
		{
			if (!meta.TryGetSingle<SoundMetaData>(out var data))
			{
				data = new SoundMetaData();
				meta.SetSingle(data);
			}
			return data;
		}

		public List<SeMetaData> SE = new List<SeMetaData>();
		public List<BgmMetaData> BGM = new List<BgmMetaData>();

		public int Priority => 0;

		public void Convert(TagParam param)
		{
			foreach (var se in SE)
			{
				if ("se_" + se.Name == param.Name)
				{
					se.Convert(param);
					return;
				}
			}
			foreach (var bgm in BGM)
			{
				if ("bgm_" + bgm.Name == param.Name)
				{
					bgm.Convert(param);
					return;
				}
			}
		}
	}

	public class SeMetaData
	{
		[Argument(Required = true)]
		public string Name;
		public string Group;
		[Argument(Required = true)]
		public string Path;
		[RateArgument]
		public float? Volume;
		[RateArgument]
		public float? Pitch;
		[RateArgument]
		public float? Pan;

		public void Convert(TagParam param)
		{
			param.TrySetNewValue(nameof(Group), Group);
			param.TrySetNewValue(nameof(Path), Path);
			param.TrySetNewValue(nameof(Volume), Volume);
			param.TrySetNewValue(nameof(Pitch), Pitch);
			param.TrySetNewValue(nameof(Pan), Pan);
		}
	}

	public class BgmMetaData
	{
		[Argument(Required = true)]
		public string Name;
		public string Group;
		[Argument(Required = true)]
		public string Path;
		[RateArgument]
		public float? Volume;
		[RateArgument]
		public float? Pitch;
		[RateArgument]
		public float? Pan;

		public void Convert(TagParam param)
		{
			param.TrySetNewValue(nameof(Group), Group);
			param.TrySetNewValue(nameof(Path), Path);
			param.TrySetNewValue(nameof(Volume), Volume);
			param.TrySetNewValue(nameof(Pitch), Pitch);
			param.TrySetNewValue(nameof(Pan), Pan);
		}
	}

	public class AutoVoiceMetaData
	{
		[Argument(Required = true)]
		[Description(
			"再生するボイスのパスです。{}で囲った以下の要素が置換されます\n" +
			"{FILE_NAME}ファイル名です\n" +
			"{BLOCK_INDEX}ラベルからのテキスト毎のインデックスです\n" +
			"{INDEX}ボイスの再生にインクリメントされるインデックスです\n" +
			"{LABEL}現在のラベルです\n" +
			"{CHARA}キャラのIDです\n" +
			"{CHARA_INDEX}キャラの発言毎にインクリメントされるインデックスです\n" +
			"{DISP_NAME}キャラの表示名です\n"
		)]
		public string Path;
		[Description("Jump時にインデックス等をリセットします")]
		public bool ResetOnJump = true;
		[Description("CHARA_INDEXを利用するか？")]
		public bool CharaIndex = false;

		KeyValueFormat m_Format;
		Dictionary<string, object> m_Dic = new Dictionary<string, object>();

		KeyValueFormat GetFormat()
		{
			if (m_Format == null)
			{
				m_Format = new KeyValueFormat(Path);
			}
			return m_Format;
		}

		public void TryAutoSet(MessageEnvData message, EnvDataUpdateParam param)
		{
			// Voiceが設定されている
			if (param.Data.GetAll<PlayVoiceEnvData>().Any() || param.Data.TryGetSingle<SkipAutoVoiceEnvData>(out _))
			{
				return;
			}
			// AutoVoiceが無効になっている
			if (param.Data.TryGetSingle(out AutoVoiceEnvData autovoice) && !autovoice.Enabled)
			{
				return;
			}
			var chara = CharaMetaData.GetKey(param.Meta, message.Chara);

			var index = autovoice.Index;
			autovoice.Index++;
			param.Data.SetSingle(autovoice);
			int charaIndex = 0;
			if (CharaIndex)
			{
				param.Data.TryGet(chara, out CharaAutoVoiceEnvData charavoice);
				charaIndex = charavoice.Index;
				charavoice.Index++;
				param.Data.Set(chara, charavoice);
			}
			var format = GetFormat();
			try
			{
				m_Dic["FILE_NAME"] = System.IO.Path.GetFileNameWithoutExtension(param.LabelInfo.FileName);
				m_Dic["BLOCK_INDEX"] = param.LabelInfo.BlockIndex;
				m_Dic["INDEX"] = index;
				m_Dic["LABEL"] = param.LabelInfo.Name;
				m_Dic["CHARA"] = chara;
				if (CharaIndex)
				{
					m_Dic["CHARA_INDEX"] = charaIndex;
				}
				m_Dic["DISP_NAME"] = message.Name;
				var path = format.Convert(m_Dic);
				param.AddCommand(VoicePlayCommand.Create(path));
			}
			finally
			{
				m_Dic.Clear();
			}
		}
	}

}