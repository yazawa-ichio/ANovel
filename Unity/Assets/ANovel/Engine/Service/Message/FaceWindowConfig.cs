using ANovel.Core;
using UnityEngine;

namespace ANovel.Service
{
	public class FaceWindowConfig
	{
		public string Name;
		public string Path;
		public float? X;
		public float? Y;
		public float? Z;
		public float? Scale;
		[SkipInjectParam]
		public ICacheHandle<Texture> Texture;

		public void PreloadTexture(string prefix, IPreLoader loader)
		{
			if (!string.IsNullOrEmpty(Path))
			{
				loader.Load<Texture>(prefix + Path);
			}
		}

		public void LoadTexture(string prefix, ResourceCache cache)
		{
			if (!string.IsNullOrEmpty(Path))
			{
				Texture = cache.Load<Texture>(prefix + Path);
			}
		}

		public Vector3 GetPos()
		{
			return new Vector3(X.GetValueOrDefault(), Y.GetValueOrDefault(), Z.GetValueOrDefault());
		}

	}

	public struct FaceWindowEnvData : IEnvDataUpdate<FaceWindowConfig>
	{
		public string Name;
		public string Path;
		public float X;
		public float Y;
		public float Z;
		public float? Scale;

		public void Update(FaceWindowConfig arg)
		{
			Name = arg.Name;
			Path = arg.Path;
			arg.X = X = arg.X.GetValueOrDefault(X);
			arg.Y = Y = arg.Y.GetValueOrDefault(Y);
			arg.Z = Z = arg.Z.GetValueOrDefault(Z);
			if (arg.Scale.HasValue)
			{
				arg.Scale = Scale = arg.Scale.GetValueOrDefault(Scale.GetValueOrDefault());
			}
			else
			{
				arg.Scale = Scale;
			}
		}

		public FaceWindowConfig CreateConfig()
		{
			return new FaceWindowConfig
			{
				Name = Name,
				Path = Path,
				X = X,
				Y = Y,
				Z = Z,
				Scale = Scale,
			};
		}

	}

}