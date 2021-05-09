using ANovel.Core;
using ANovel.Serialization;
using UnityEngine;

namespace ANovel.Service
{
	public partial class LayoutConfig
	{

		public static void SetEvnData(string name, IEnvData data, LayoutConfig config)
		{
			data.Set(name, new LayoutPosEnvData(config));
			data.Set(name, new LayoutAngleEnvData(config));
			data.Set(name, new LayoutSizeEnvData(config));
			data.Set(name, new LayoutColorEnvData(config));
			data.Set(name, new LayoutLevelEnvData(config));
		}

		public static void UpdateEvnData(string name, IEnvData data, LayoutConfig config)
		{
			data.Update<LayoutPosEnvData, LayoutConfig>(name, config);
			data.Update<LayoutAngleEnvData, LayoutConfig>(name, config);
			data.Update<LayoutSizeEnvData, LayoutConfig>(name, config);
			data.Update<LayoutColorEnvData, LayoutConfig>(name, config);
			data.Update<LayoutLevelEnvData, LayoutConfig>(name, config);
		}

		public static void DeleteEvnData(string name, IEnvData data)
		{
			data.Delete<LayoutPosEnvData>(name);
			data.Delete<LayoutAngleEnvData>(name);
			data.Delete<LayoutSizeEnvData>(name);
			data.Delete<LayoutColorEnvData>(name);
			data.Delete<LayoutLevelEnvData>(name);
		}

		public static LayoutConfig Restore(string name, IEnvDataHolder data)
		{
			LayoutConfig config = new LayoutConfig();
			data.Get<LayoutPosEnvData>(name).Update(config);
			data.Get<LayoutAngleEnvData>(name).Update(config);
			data.Get<LayoutSizeEnvData>(name).Update(config);
			data.Get<LayoutColorEnvData>(name).Update(config);
			data.Get<LayoutLevelEnvData>(name).Update(config);
			return config;
		}

		public struct LayoutPosEnvData : IEnvDataUpdate<LayoutConfig>, IScreenChildEnvData, IDefaultValueSerialization
		{
			public bool IsDefault => Equals(default);

			public float PosX;
			public float PosY;
			public float PosZ;

			public LayoutPosEnvData(LayoutConfig config)
			{
				PosX = config.PosX.GetValueOrDefault();
				PosY = config.PosY.GetValueOrDefault();
				PosZ = config.PosZ.GetValueOrDefault();
			}

			public LayoutPosEnvData(float posX, float posY, float posZ)
			{
				PosX = posX;
				PosY = posY;
				PosZ = posZ;
			}

			public bool Equals(LayoutPosEnvData data)
			{
				return PosX == data.PosX && PosY == data.PosY && PosZ == data.PosZ;
			}

			public void Update(LayoutConfig arg)
			{
				arg.PosX = PosX = arg.PosX.GetValueOrDefault(PosX);
				arg.PosY = PosY = arg.PosY.GetValueOrDefault(PosY);
				arg.PosZ = PosZ = arg.PosZ.GetValueOrDefault(PosZ);
			}
		}

		public struct LayoutSizeEnvData : IEnvDataUpdate<LayoutConfig>, IScreenChildEnvData, IDefaultValueSerialization
		{
			public bool IsDefault => Equals(default);

			public ScreenMatchMode? ScreenMatch;
			public float? Scale;
			public float? ScaleX;
			public float? ScaleY;
			public float? Width;
			public float? Height;

			public LayoutSizeEnvData(LayoutConfig layout)
			{
				ScreenMatch = null;
				Scale = null;
				ScaleX = null;
				ScaleY = null;
				Width = null;
				Height = null;
				Update(layout);
			}

			public void Update(LayoutConfig layout)
			{
				if (layout.ScreenMatch.HasValue)
				{
					if (ScreenMatch != layout.ScreenMatch)
					{
						ScreenMatch = layout.ScreenMatch;
						Scale = null;
						ScaleX = null;
						ScaleY = null;
						Width = null;
						Height = null;
					}
				}
				else
				{
					layout.ScreenMatch = ScreenMatch;
				}
				if (layout.Scale.HasValue)
				{
					Scale = layout.Scale;
					ScaleX = null;
					ScaleY = null;
					Width = null;
					Height = null;
				}
				if (layout.ScaleX.HasValue)
				{
					ScaleX = layout.ScaleX;
					Width = null;
				}
				if (layout.ScaleY.HasValue)
				{
					ScaleY = layout.ScaleY;
					Height = null;
				}
				if (layout.Width.HasValue)
				{
					ScaleX = null;
					Width = layout.Width;
				}
				if (layout.Height.HasValue)
				{
					ScaleY = null;
					Height = layout.Height;
				}
				layout.Scale = Scale;
				layout.ScaleX = ScaleX;
				layout.ScaleY = ScaleY;
				layout.Width = Width;
				layout.Height = Height;
			}

			public bool Equals(LayoutSizeEnvData data)
			{
				return ScreenMatch == data.ScreenMatch && Scale == data.Scale && ScaleX == data.ScaleX && ScaleY == data.ScaleY && Width == data.Width && Height == data.Height;
			}
		}

		public struct LayoutAngleEnvData : IEnvDataUpdate<LayoutConfig>, IScreenChildEnvData, IDefaultValueSerialization
		{
			public bool IsDefault => Equals(default);

			public float AngleX;
			public float AngleY;
			public float AngleZ;

			public LayoutAngleEnvData(LayoutConfig config)
			{
				AngleX = config.AngleX.GetValueOrDefault();
				AngleY = config.AngleY.GetValueOrDefault();
				AngleZ = config.AngleZ.GetValueOrDefault();
			}

			public LayoutAngleEnvData(float angleX, float angleY, float angleZ)
			{
				AngleX = angleX;
				AngleY = angleY;
				AngleZ = angleZ;
			}

			public bool Equals(LayoutAngleEnvData data)
			{
				return AngleX == data.AngleX && AngleY == data.AngleY && AngleZ == data.AngleZ;
			}

			public void Update(LayoutConfig arg)
			{
				arg.AngleX = AngleX = arg.AngleX.GetValueOrDefault(AngleX);
				arg.AngleY = AngleY = arg.AngleY.GetValueOrDefault(AngleY);
				arg.AngleZ = AngleZ = arg.AngleZ.GetValueOrDefault(AngleZ);
			}
		}

		public struct LayoutColorEnvData : IEnvDataUpdate<LayoutConfig>, IScreenChildEnvData, IDefaultValueSerialization
		{
			public bool IsDefault => Equals(default);

			public Color Color;
			public float Opacity;

			public LayoutColorEnvData(LayoutConfig config)
			{
				Color = config.Color.GetValueOrDefault(Color.white);
				Opacity = config.Opacity.GetValueOrDefault(1f);
			}

			public LayoutColorEnvData(Color? color, float? opacity)
			{
				Color = color.GetValueOrDefault(Color.white);
				Opacity = opacity.GetValueOrDefault(1f);
			}

			public bool Equals(LayoutColorEnvData data)
			{
				return Color.Equals(data.Color) && Opacity == data.Opacity;
			}

			public void Update(LayoutConfig arg)
			{
				arg.Color = Color = arg.Color.GetValueOrDefault(Color);
				arg.Opacity = Opacity = arg.Opacity.GetValueOrDefault(Opacity);
			}
		}

		public struct LayoutLevelEnvData : IEnvDataUpdate<LayoutConfig>, IScreenChildEnvData, IDefaultValueSerialization
		{
			public bool IsDefault => Equals(default);

			public string Level;

			public LayoutLevelEnvData(LayoutConfig config)
			{
				Level = config.Level;
			}

			public LayoutLevelEnvData(string level)
			{
				Level = level;
			}

			public bool Equals(LayoutLevelEnvData data)
			{
				return Level == data.Level;
			}

			public void Update(LayoutConfig arg)
			{
				if (string.IsNullOrEmpty(arg.Level))
				{
					arg.Level = Level;
				}
				else
				{
					Level = arg.Level;
				}
			}
		}

	}
}