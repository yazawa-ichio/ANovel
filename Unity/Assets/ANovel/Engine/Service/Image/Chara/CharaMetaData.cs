using System.Collections.Generic;

namespace ANovel.Engine
{

	public class CharaMetaData
	{
		static readonly string s_DefaultLevelName = ANovel.Level.Center.ToString();

		public static string GetKey(IMetaData meta, string name)
		{
			if (meta.TryGetSingle(out CharaCommonMetaData common))
			{
				if (common.DispNameToName.TryGetValue(name, out var conv))
				{
					return conv;
				}
			}
			return name;
		}

		public static CharaMetaData Get(IMetaData meta, string name)
		{
			meta.TryGetSingle(out CharaCommonMetaData common);
			if (meta.TryGet(name, out CharaMetaData data))
			{
				data.Common = common;
				return data;
			}
			if (common == null)
			{
				throw new System.Exception($"not found MetaData {name}");
			}
			return common.GetDefaultMetaData();
		}

		public string Path;
		public string FaceWindowPath;
		public string DefaultFace;
		public string DefaultPose;

		public float? FaceWindowX;
		public float? FaceWindowY;
		public float? FaceWindowZ;
		public float? FaceWindowScale;

		[SkipArgument, System.NonSerialized]
		public CharaCommonMetaData Common;
		[SkipArgument]
		public List<DefineCharaPoseParam> Pose = new List<DefineCharaPoseParam>();
		[SkipArgument]
		public List<DefineCharaParam> Face = new List<DefineCharaParam>();
		[SkipArgument]
		public List<DefineCharaLevelParam> Level = new List<DefineCharaLevelParam>();

		public string GetPath(string name, CharaObjectEnvData data)
		{
			return GetPathImpl(Path ?? Common?.Path, name, data);
		}

		public string GetFaceWindowPath(string name, CharaObjectEnvData data)
		{
			return GetPathImpl(FaceWindowPath ?? Common?.FaceWindowPath, name, data);
		}

		public void UpdateLayout(CharaObjectEnvData data, LayoutConfig layout)
		{
			// ポーズデータ分ずらす
			var pose = GetPoseData(data.Pose);
			var level = GetLevelData(data.Level);
			layout.Scale = layout.Scale.GetValueOrDefault(1f) * pose.Scale * level.Scale;
			layout.PosX = layout.PosX.GetValueOrDefault() + pose.OffsetX + level.OffsetX;
			layout.PosY = layout.PosY.GetValueOrDefault() + pose.OffsetY + level.OffsetY;
			layout.PosZ = layout.PosZ.GetValueOrDefault() + pose.OffsetZ + level.OffsetZ;
		}

		public DefineCharaPoseParam GetPoseData(string pose)
		{
			if (string.IsNullOrEmpty(pose))
			{
				pose = GetDefaultPose();
			}
			return GetParam(pose, Pose, Common?.Pose);
		}

		public DefineCharaLevelParam GetLevelData(string level)
		{
			if (string.IsNullOrEmpty(level))
			{
				level = s_DefaultLevelName;
			}
			return GetParam(level, Level, Common?.Level);
		}

		public T GetParam<T>(string value, List<T> list, List<T> common) where T : DefineCharaParam, new()
		{
			foreach (var data in list)
			{
				if (data.Value == value)
				{
					return data;
				}
			}
			if (common != null)
			{
				foreach (var data in common)
				{
					if (data.Value == value)
					{
						return data;
					}
				}
			}
			return new T
			{
				Value = value,
			};
		}


		public void UpdateFaceWindow(string name, CharaObjectEnvData data, FaceWindowConfig config)
		{
			config.Name = name;
			config.Path = GetFaceWindowPath(name, data);
			var pose = GetPoseData(data.Pose);
			if (pose != null)
			{
				if (!config.X.HasValue) config.X = pose.FaceWindowX;
				if (!config.Y.HasValue) config.Y = pose.FaceWindowY;
				if (!config.Z.HasValue) config.Z = pose.FaceWindowZ;
				if (!config.Scale.HasValue) config.Scale = pose.FaceWindowScale;
			}
			{
				if (!config.X.HasValue) config.X = FaceWindowX;
				if (!config.Y.HasValue) config.Y = FaceWindowY;
				if (!config.Z.HasValue) config.Z = FaceWindowZ;
				if (!config.Scale.HasValue) config.Scale = FaceWindowScale;
			}
			if (Common != null)
			{
				if (!config.Scale.HasValue) config.Scale = Common.FaceWindowScale;
			}
		}

		string GetDefaultFace()
		{
			var defFace = DefaultFace ?? Common?.DefaultFace; ;
			return GetDefaultValue(defFace, Face, Common?.Face);
		}

		string GetDefaultPose()
		{
			var defPose = DefaultPose ?? Common?.DefaultPose;
			return GetDefaultValue(defPose, Pose, Common?.Pose);
		}

		string GetDefaultValue<T>(string def, List<T> list, List<T> common) where T : DefineCharaParam
		{
			var defPose = DefaultPose ?? Common?.DefaultPose;
			foreach (var data in list)
			{
				if (data.Key == def)
				{
					return data.Value;
				}
			}
			if (common != null)
			{
				foreach (var data in common)
				{
					if (data.Key == def)
					{
						return data.Value;
					}
				}
			}
			return defPose;
		}

		string GetPathImpl(string path, string name, CharaObjectEnvData data)
		{
			var face = data.Face;
			var pose = data.Pose;
			var level = data.Level;
			if (string.IsNullOrEmpty(face))
			{
				face = GetDefaultFace();
			}
			if (string.IsNullOrEmpty(pose))
			{
				pose = GetDefaultPose();
			}
			if (string.IsNullOrEmpty(level))
			{
				level = s_DefaultLevelName;
			}
			return path?.Replace("{FACE}", face).Replace("{POSE}", pose).Replace("{LEVEL}", level).Replace("{NAME}", name);
		}

	}

}