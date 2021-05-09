using ANovel.Core;
using UnityEngine;

namespace ANovel.Service
{

	public enum ScreenMatchMode
	{
		None,
		Fit,
		Expand,
		Shrink,
	}

	public partial class LayoutConfig
	{
		[CommandField(KeyName = "x")]
		public float? PosX;
		[CommandField(KeyName = "y")]
		public float? PosY;
		[CommandField(KeyName = "z")]
		public float? PosZ;
		public ScreenMatchMode? ScreenMatch;
		public float? Scale;
		public float? ScaleX;
		public float? ScaleY;
		[CommandField(KeyName = "w")]
		public float? Width;
		[CommandField(KeyName = "h")]
		public float? Height;

		public float? AngleX;
		public float? AngleY;
		public float? AngleZ;

		[CommandField(Formatter = typeof(ColorFormatter))]
		public Color? Color;
		public float? Opacity;

		public string Level;

		public ImageLayout GetLayout(ImageLayout layout, ICacheHandle<Texture> texture, Vector2 screenSize)
		{
			Vector2? baseSize = null;
			if (texture != null)
			{
				baseSize = new Vector2(texture.Value.width, texture.Value.height);
			}
			return GetLayout(layout, baseSize, screenSize);
		}

		public ImageLayout GetLayout(ImageLayout layout, Vector2? texSize, Vector2 screenSize)
		{
			var baseSize = GetMatchSize(texSize, screenSize);
			return GetLayout(layout, baseSize);
		}

		Vector2? GetMatchSize(Vector2? baseSize, Vector2 screenSize)
		{
			if (!ScreenMatch.HasValue)
			{
				return baseSize;
			}
			if (!baseSize.HasValue)
			{
				if (ScreenMatch.Value == ScreenMatchMode.None)
				{
					return baseSize;
				}
				else
				{
					return screenSize;
				}
			}
			var size = baseSize.Value;
			switch (ScreenMatch.Value)
			{
				case ScreenMatchMode.Fit:
					return screenSize;
				case ScreenMatchMode.Expand:
					{
						var scaleFactor = Mathf.Min(screenSize.x / size.x, screenSize.y / size.y);
						return size * scaleFactor;
					}
				case ScreenMatchMode.Shrink:
					{
						var scaleFactor = Mathf.Max(screenSize.x / size.x, screenSize.y / size.y);
						return size * scaleFactor;
					}
			}
			return baseSize;
		}

		ImageLayout GetLayout(ImageLayout layout, Vector2? baseSize)
		{
			layout.Pos.x = PosX.GetValueOrDefault(layout.Pos.x);
			layout.Pos.y = PosY.GetValueOrDefault(layout.Pos.y);
			layout.Pos.z = PosZ.GetValueOrDefault(layout.Pos.z);
			layout.Size = baseSize.GetValueOrDefault(layout.Size);
			if (Scale.HasValue)
			{
				layout.Size *= Scale.Value;
			}
			if (ScaleX.HasValue)
			{
				layout.Size.x *= ScaleX.Value;
			}
			if (ScaleY.HasValue)
			{
				layout.Size.y *= ScaleY.Value;
			}
			layout.Size.x = Width.GetValueOrDefault(layout.Size.x);
			layout.Size.y = Height.GetValueOrDefault(layout.Size.y);
			layout.Angle.x = AngleX.GetValueOrDefault(layout.Angle.x);
			layout.Angle.y = AngleY.GetValueOrDefault(layout.Angle.y);
			layout.Angle.z = AngleZ.GetValueOrDefault(layout.Angle.z);

			layout.Color = Color.GetValueOrDefault(layout.Color);
			layout.Opacity = Opacity.GetValueOrDefault(layout.Opacity);

			return layout;
		}

	}


}