using ANovel.Core;
using System.Collections.Generic;

namespace ANovel.Engine
{
	public abstract class DefineCharaBaseCommand : PreProcess, IParamConverter
	{
		[Argument(Required = true)]
		string m_Name = default;
		[Argument(Required = true)]
		string m_DispName = default;

		MetaData m_Meta;

		int IParamConverter.Priority => 0;

		protected abstract CharaMetaData Param { get; }

		bool CheckPath(PreProcessResult result)
		{
			if (result.Meta.TryGetSingle<CharaCommonMetaData>(out var common) && !string.IsNullOrEmpty(common.Path))
			{
				return true;
			}
			return !string.IsNullOrEmpty(Param.Path);
		}

		public override void Result(PreProcessResult result)
		{
			if (Param == null || !CheckPath(result))
			{
				throw new System.Exception("パラメーターが不正です");
			}
			result.Meta.Add(m_Name, Param);
			result.Converters.Add(this);
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
				ConvertParam(param, "face", Param.Face);
				ConvertParam(param, "pose", Param.Pose);
				ConvertParam(param, "level", Param.Level);
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
				if (param.ContainsKey("show"))
				{
					param.Name = "chara";
				}
				else if (param.ContainsKey("hide"))
				{
					param.Name = "chara_hide";
				}
				else if (param.ContainsKey("change"))
				{
					param.Name = "chara_change";
				}
				else if (param.ContainsKey("layout"))
				{
					param.Name = "chara_layout";
				}
				else if (param.ContainsKey("action"))
				{
					param.Name = "chara_action";
				}
				else
				{
					param.Name = "chara";
				}
				param.AddValue("name", m_Name);
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
						param.AddValue(key, defParam.Value);
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
						param.AddValue(key, defParam.Value);
						return;
					}
				}
			}
		}

	}

#if ANOVEL_DEFINE_IMPORT
	[TagName("import_chara")]
	public class ImportCharaCommand : DefineCharaBaseCommand, IImportText
	{
		[Argument(Required = true)]
		string m_Path = default;
		[InjectArgument]
		CharaMetaData m_Param = default;

		string IImportText.Path => m_Path;

		protected override CharaMetaData Param => m_Param;

		void IImportText.Import(string text)
		{
			m_Param = UnityEngine.JsonUtility.FromJson<CharaMetaData>(text);
		}

	}
#endif

	[TagName("define_chara")]
	[ReplaceTagDefine("@{dispname}", "@chara name=\"{name}\"")]
	[ReplaceTagDefine("@{dispname}", "@chara_hide name=\"{name}\"", SecondaryKey = "hide")]
	[ReplaceTagDefine("@{dispname}", "@chara_change name=\"{name}\"", SecondaryKey = "change")]
	[ReplaceTagDefine("@{dispname}", "@chara_layout name=\"{name}\"", SecondaryKey = "layout")]
	[ReplaceTagDefine("@{dispname}", "@chara name=\"{name}\"", Label = "{name}({dispname})")]
	[ReplaceTagDefine("@{dispname}", "@chara_hide name=\"{name}\"", SecondaryKey = "hide", Label = "{name}({dispname})")]
	[ReplaceTagDefine("@{dispname}", "@chara_change name=\"{name}\"", SecondaryKey = "change", Label = "{name}({dispname})")]
	[ReplaceTagDefine("@{dispname}", "@chara_layout name=\"{name}\"", SecondaryKey = "layout", Label = "{name}({dispname})")]
	[ReplaceTagDefine("@{dispname}", "@chara_action name=\"{name}\"", SecondaryKey = "action", Label = "{name}({dispname})")]
	public class DefineCharaCommand : DefineCharaBaseCommand
	{
		[InjectArgument]
		CharaMetaData m_Param = default;

		protected override CharaMetaData Param => m_Param;
	}

	[TagName("define_chara_face")]
	public class DefineCharaFaceCommand : PreProcess
	{
		[Argument(Required = true)]
		string m_Name = default;
		[InjectArgument]
		DefineCharaParam m_Param = new DefineCharaParam();

		public override void Result(PreProcessResult result)
		{
			var meta = result.Meta.Get<CharaMetaData>(m_Name);
			meta.Face.Add(m_Param);
		}
	}

	[TagName("define_chara_pose")]
	public class DefineCharaPoseCommand : PreProcess
	{
		[Argument(Required = true)]
		string m_Name = default;
		[InjectArgument]
		DefineCharaPoseParam m_Param = new DefineCharaPoseParam();

		public override void Result(PreProcessResult result)
		{
			var meta = result.Meta.Get<CharaMetaData>(m_Name);
			meta.Pose.Add(m_Param);
		}
	}

	[TagName("define_chara_level")]
	public class DefineCharaLevelCommand : PreProcess
	{
		[Argument(Required = true)]
		string m_Name = default;
		[InjectArgument]
		DefineCharaLevelParam m_Param = new DefineCharaLevelParam();

		public override void Result(PreProcessResult result)
		{
			var meta = result.Meta.Get<CharaMetaData>(m_Name);
			meta.Level.Add(m_Param);
		}
	}


	[TagName("define_chara_common")]
	public class DefineCharaCommonCommand : PreProcess
	{
		public static DefineCharaLevelCommand Get(IMetaData meta)
		{
			return meta.Get<DefineCharaLevelCommand>(nameof(DefineCharaCommonCommand));
		}

		[InjectArgument]
		CharaCommonMetaData m_Param = default;

		public override void Result(PreProcessResult result)
		{
			if (!result.Converters.Contains(m_Param))
			{
				result.Converters.Add(m_Param);
			}
			result.Meta.SetSingle(m_Param);
		}

	}

#if ANOVEL_DEFINE_IMPORT
	[TagName("import_chara_common")]
	public class ImportCharaCommonCommand : PreProcess, IImportText
	{
		public static DefineCharaLevelCommand Get(IMetaData meta)
		{
			return meta.Get<DefineCharaLevelCommand>(nameof(DefineCharaCommonCommand));
		}

		[Argument(Required = true)]
		string m_Path = default;
		CharaCommonMetaData m_Param = default;

		string IImportText.Path => m_Path;

		void IImportText.Import(string text)
		{
			m_Param = UnityEngine.JsonUtility.FromJson<CharaCommonMetaData>(text);
		}

		public override void Result(PreProcessResult result)
		{
			if (!result.Converters.Contains(m_Param))
			{
				result.Converters.Add(m_Param);
			}
			result.Meta.SetSingle(m_Param);
		}

	}
#endif

	[TagName("define_chara_common_pose")]
	public class DefineCharaCommonPoseCommand : PreProcess
	{
		[InjectArgument]
		DefineCharaPoseParam m_Param = new DefineCharaPoseParam();

		public override void Result(PreProcessResult result)
		{
			var meta = result.Meta.GetOrCreateSingle<CharaCommonMetaData>();
			if (!result.Converters.Contains(meta))
			{
				result.Converters.Add(meta);
			}
			meta.Pose.Add(m_Param);
		}
	}

	[TagName("define_chara_common_face")]
	public class DefineCharaCommonFaceCommand : PreProcess
	{
		[InjectArgument]
		DefineCharaParam m_Param = new DefineCharaParam();

		public override void Result(PreProcessResult result)
		{
			var meta = result.Meta.GetOrCreateSingle<CharaCommonMetaData>();
			if (!result.Converters.Contains(meta))
			{
				result.Converters.Add(meta);
			}
			meta.Face.Add(m_Param);
		}
	}

	[TagName("define_chara_common_level")]
	public class DefineCharaCommonLevelCommand : PreProcess
	{
		[InjectArgument]
		DefineCharaLevelParam m_Param = new DefineCharaLevelParam();

		public override void Result(PreProcessResult result)
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