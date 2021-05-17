using ANovel.Commands;
using ANovel.Core;
using ANovel.Service.Sound;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Service
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
				if (se.Name == param.Name)
				{
					se.Convert(param);
					return;
				}
			}
			foreach (var se in BGM)
			{
				if (se.Name == param.Name)
				{
					se.Convert(param);
					return;
				}
			}
		}
	}

	public class SeMetaData
	{
		[CommandField(Required = true)]
		public string Name;
		public string Group;
		[CommandField(Required = true)]
		public string Path;
		public float? Volume;
		public float? Pitch;
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
		[CommandField(Required = true)]
		public string Name;
		public string Group;
		[CommandField(Required = true)]
		public string Path;
		public float? Volume;
		public float? Pitch;
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
		[CommandField(Required = true)]
		public string Path;
		public bool ResetOnJump = true;
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