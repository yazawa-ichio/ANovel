using ANovel.Core;
using System.Collections.Generic;

namespace ANovel.Engine
{

	[PreProcessName("define_chara")]
	public class DefineCharaCommand : PreProcess, IImportText, IParamConverter
	{
		[CommandField(Required = true)]
		string m_Name = default;
		[CommandField]
		string m_DispName = default;
		[InjectParam]
		CharaMetaData m_Param = default;
		[CommandField]
		string m_Import = default;

		MetaData m_Meta;

		int IParamConverter.Priority => 0;

		bool IImportText.Enabled => !string.IsNullOrEmpty(m_Import);

		string IImportText.Path => m_Import;

		void IImportText.Import(string text)
		{
			m_Param = UnityEngine.JsonUtility.FromJson<CharaMetaData>(text);
		}

		bool CheckPath(PreProcessor.Result result)
		{
			if (result.Meta.TryGetSingle<CharaCommonMetaData>(out var common) && !string.IsNullOrEmpty(common.Path))
			{
				return false;
			}
			return !string.IsNullOrEmpty(m_Param.Path);
		}

		public override void Result(PreProcessor.Result result)
		{
			if (m_Param == null || !CheckPath(result))
			{
				throw new System.Exception("パラメーターが不正です");
			}
			result.Converters.Add(this);
			result.Meta.Add(m_Name, m_Param);
			if (!string.IsNullOrEmpty(m_DispName))
			{
				var common = result.Meta.GetOrCreateSingle<CharaCommonMetaData>();
				common.DispNameToName[m_DispName] = m_Name;
			}
			m_Meta = result.Meta;
		}

		public void Convert(TagParam param)
		{
			if (CharaCommonMetaData.ConvertList.Contains(param.Name) || param.Name == m_Name || param.Name == m_DispName)
			{
				ConvertParam(param, "face", m_Param.Face);
				ConvertParam(param, "pose", m_Param.Pose);
				ConvertParam(param, "level", m_Param.Level);
				if (m_Meta.TryGetSingle<CharaCommonMetaData>(out var common))
				{
					common.PreConvert(param);
				}
				ConvertName(param, m_Name);
				ConvertName(param, m_DispName);
			}
		}

		void ConvertName(TagParam param, string name)
		{
			if (param.Name == name)
			{
				if (param.ContainsKey("hide"))
				{
					param.Name = "chara_hide";
				}
				else if (param.ContainsKey("change"))
				{
					param.Name = "chara_change";
				}
				else if (!param.ContainsKey("face") && !param.ContainsKey("pose") && !param.ContainsKey("level"))
				{
					param.Name = "chara_control";
				}
				else
				{
					param.Name = "chara";
				}
				param["name"] = m_Name;
			}
		}

		void ConvertParam(TagParam param, string key, IEnumerable<DefineCharaParam> defParamList)
		{
			if (!param.ContainsKey(key))
			{
				foreach (var defParam in defParamList)
				{

					if (param.ContainsKey(defParam.Key) || param.ContainsKey(defParam.Value))
					{
						param[key] = defParam.Value;
						return;
					}
				}
			}
			else
			{
				var value = param[key];
				foreach (var defParam in defParamList)
				{
					if (param.ContainsKey(value))
					{
						param[key] = defParam.Value;
						return;
					}
				}
			}
		}

	}

	[PreProcessName("define_chara_face")]
	public class DefineCharaFaceCommand : PreProcess
	{
		[CommandField]
		string m_Name = default;
		[InjectParam]
		DefineCharaParam m_Param = new DefineCharaParam();

		public override void Result(PreProcessor.Result result)
		{
			if (!string.IsNullOrEmpty(m_Name))
			{
				var meta = result.Meta.Get<CharaMetaData>(m_Name);
				meta.Face.Add(m_Param);
			}
			else
			{
				var meta = result.Meta.GetOrCreateSingle<CharaCommonMetaData>();
				if (!result.Converters.Contains(meta))
				{
					result.Converters.Add(meta);
				}
				meta.Face.Add(m_Param);
			}
		}
	}

	[PreProcessName("define_chara_pose")]
	public class DefineCharaPoseCommand : PreProcess
	{
		[CommandField]
		string m_Name = default;
		[InjectParam]
		DefineCharaPoseParam m_Param = new DefineCharaPoseParam();

		public override void Result(PreProcessor.Result result)
		{
			if (!string.IsNullOrEmpty(m_Name))
			{
				var meta = result.Meta.Get<CharaMetaData>(m_Name);
				meta.Pose.Add(m_Param);
			}
			else
			{
				var meta = result.Meta.GetOrCreateSingle<CharaCommonMetaData>();
				if (!result.Converters.Contains(meta))
				{
					result.Converters.Add(meta);
				}
				meta.Pose.Add(m_Param);
			}
		}
	}

	[PreProcessName("define_chara_level")]
	public class DefineCharaLevelCommand : PreProcess
	{
		[CommandField]
		string m_Name = default;
		[InjectParam]
		DefineCharaLevelParam m_Param = new DefineCharaLevelParam();

		public override void Result(PreProcessor.Result result)
		{
			if (!string.IsNullOrEmpty(m_Name))
			{
				var meta = result.Meta.Get<CharaMetaData>(m_Name);
				meta.Level.Add(m_Param);
			}
			else
			{
				var meta = result.Meta.GetOrCreateSingle<CharaCommonMetaData>();
				if (!result.Converters.Contains(meta))
				{
					result.Converters.Add(meta);
				}
				meta.Level.Add(m_Param);
			}
		}
	}

	[PreProcessName("define_chara_common")]
	public class DefineCharaCommonCommand : PreProcess, IImportText
	{
		public static DefineCharaLevelCommand Get(IMetaData meta)
		{
			return meta.Get<DefineCharaLevelCommand>(nameof(DefineCharaCommonCommand));
		}

		[InjectParam]
		CharaCommonMetaData m_Param = default;
		[CommandField]
		string m_Import = default;

		bool IImportText.Enabled => !string.IsNullOrEmpty(m_Import);

		string IImportText.Path => m_Import;

		void IImportText.Import(string text)
		{
			m_Param = UnityEngine.JsonUtility.FromJson<CharaCommonMetaData>(text);
		}

		public override void Result(PreProcessor.Result result)
		{
			if (!result.Converters.Contains(m_Param))
			{
				result.Converters.Add(m_Param);
			}
			result.Meta.SetSingle(m_Param);
		}

	}

}