using ANovel.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANovel.Engine
{
	public struct ImageLayout
	{
		public Vector3 Pos;
		public Vector2 Size;
		public Vector3 Angle;
		public Color Color;
		public float Opacity;

		public float Get(ImageParamType type)
		{
			switch (type)
			{
				case ImageParamType.PosX:
					return Pos.x;
				case ImageParamType.PosY:
					return Pos.y;
				case ImageParamType.PosZ:
					return Pos.z;
				case ImageParamType.SizeX:
					return Size.x;
				case ImageParamType.SizeY:
					return Size.y;
				case ImageParamType.AngleX:
					return Angle.x;
				case ImageParamType.AngleY:
					return Angle.y;
				case ImageParamType.AngleZ:
					return Angle.z;
				case ImageParamType.ColorR:
					return Color.r;
				case ImageParamType.ColorG:
					return Color.g;
				case ImageParamType.ColorB:
					return Color.b;
				case ImageParamType.Opacity:
					return Opacity;
			}
			throw new ArgumentException($"not found Float Param type {type}", "type");
		}

		public void Set(ImageParamType type, float value)
		{
			switch (type)
			{
				case ImageParamType.PosX:
					Pos.x = value;
					break;
				case ImageParamType.PosY:
					Pos.y = value;
					break;
				case ImageParamType.PosZ:
					Pos.z = value;
					break;
				case ImageParamType.SizeX:
					Size.x = value;
					break;
				case ImageParamType.SizeY:
					Size.y = value;
					break;
				case ImageParamType.AngleX:
					Angle.x = value;
					break;
				case ImageParamType.AngleY:
					Angle.y = value;
					break;
				case ImageParamType.AngleZ:
					Angle.z = value;
					break;
				case ImageParamType.ColorR:
					Color.r = value;
					break;
				case ImageParamType.ColorG:
					Color.g = value;
					break;
				case ImageParamType.ColorB:
					Color.b = value;
					break;
				case ImageParamType.Opacity:
					Opacity = value;
					break;
				default:
					throw new ArgumentException($"not found Float Param type {type}", "type");
			}
		}

		public bool CanTransition(in ImageLayout layout)
		{
			return Pos == layout.Pos && Size == layout.Size && Angle == layout.Angle;
		}


		static readonly ImageParamType[] s_Types = (ImageParamType[])Enum.GetValues(typeof(ImageParamType));
		public ImageObjectParamAnimConfig[] GetAnims(Millisecond time, Easing? easing, ImageLayout next)
		{
			using (ListPool<ImageObjectParamAnimConfig>.Use(out var list))
			{
				foreach (var type in s_Types)
				{
					if (Get(type) != next.Get(type))
					{
						list.Add(new ImageObjectParamAnimConfig
						{
							Type = type,
							Value = next.Get(type),
							Time = time,
							Easing = easing,
						});
					}
				}
				return list.ToArray();
			}
		}

		public IEnumerable<(ImageParamType type, float from, float to)> GetDiff(ImageLayout next)
		{
			foreach (var type in s_Types)
			{
				var f = Get(type);
				var t = next.Get(type);
				if (t != f)
				{
					yield return (type, f, t);
				}
			}
		}



	}
}