using ANovel.Core;
using UnityEngine;

namespace ANovel.Service
{

	public partial class ImageObjectConfig
	{
		[CommandField(Required = true)]
		public string Path;
		public Millisecond Time = Millisecond.FromSecond(0.2f);
		public float Vague = 0.2f;
		public string Rule;

		[SkipInjectParam]
		public ICacheHandle<Texture> Texture;

		[SkipInjectParam]
		public ICacheHandle<Texture> RuleTexture;

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

		public void PreloadRule(PathConfig path, IPreLoader loader)
		{
			PreloadRule(path.GetRule(""), loader);
		}

		public void PreloadRule(string prefix, IPreLoader loader)
		{
			if (!string.IsNullOrEmpty(Rule))
			{
				loader.Load<Texture>(prefix + Path);
			}
		}

		public void LoadRule(PathConfig path, ResourceCache cache)
		{
			LoadRule(path.GetRule(""), cache);
		}

		public void LoadRule(string prefix, ResourceCache cache)
		{
			if (!string.IsNullOrEmpty(Rule))
			{
				RuleTexture = cache.Load<Texture>(prefix + Path);
			}
		}

		public static ImageObjectConfig Restore(ImageObjectEnvData data, string prefix, ResourceCache cache)
		{
			var config = new ImageObjectConfig();
			config.Path = data.Path;
			config.LoadTexture(prefix, cache);
			return config;
		}

	}

	public struct ImageObjectEnvData : IEnvDataValue<ImageObjectEnvData>, IEnvDataUpdate<ImageObjectConfig>, IScreenChildEnvData
	{

		public string Path;

		public ImageObjectEnvData(ImageObjectConfig conf)
		{
			Path = conf.Path;
		}

		public ImageObjectEnvData(string path)
		{
			Path = path;
		}

		public bool Equals(ImageObjectEnvData other)
		{
			return Path == other.Path;
		}

		public void Update(ImageObjectConfig arg)
		{
			Path = arg.Path;
		}

	}

	public class ImageObjectParamAnimConfig
	{
		public ImageParamType Type;
		public float Value;
		public Millisecond Time;
		public Easing? Easing;
	}

}