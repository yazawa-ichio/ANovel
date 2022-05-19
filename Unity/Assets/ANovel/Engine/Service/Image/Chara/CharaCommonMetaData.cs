using ANovel.Core;
using System.Collections.Generic;

namespace ANovel.Engine
{
	public class CharaCommonMetaData : IParamConverter
	{
		public string Path;
		public string FaceWindowPath;
		public string DefaultFace;
		public string DefaultPose;
		public float? FaceWindowScale = 1f;
		[SkipArgument]
		public List<DefineCharaPoseParam> Pose = new List<DefineCharaPoseParam>();
		[SkipArgument]
		public List<DefineCharaParam> Face = new List<DefineCharaParam>();
		[SkipArgument]
		public List<DefineCharaLevelParam> Level = new List<DefineCharaLevelParam>();
		[SkipArgument]
		public Dictionary<string, string> DispNameToName = new Dictionary<string, string>();

		CharaMetaData m_Default;
		public CharaMetaData GetDefaultMetaData()
		{
			if (m_Default == null)
			{
				m_Default = new CharaMetaData();
				m_Default.Common = this;
			}
			return m_Default;
		}

		public int Priority => -10;

		public static readonly HashSet<string> ConvertList = new HashSet<string>
		{
			"chara",
			"chara_change",
			"chara_face_window",
		};

		public void Convert(TagParam param)
		{
			if (ConvertList.Contains(param.Name) && !param.ContainsKey("@pre_convert_chara_common_meta"))
			{
				ConvertParam(param, "face", Face);
				ConvertParam(param, "pose", Pose);
				ConvertParam(param, "level", Level);
			}
		}

		public void PreConvert(TagParam param)
		{
			ConvertParam(param, "face", Face);
			ConvertParam(param, "pose", Pose);
			ConvertParam(param, "level", Level);
			param.AddValue("@pre_convert_chara_common_meta", "true");
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
}