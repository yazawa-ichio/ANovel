using UnityEngine;

namespace ANovel.Engine
{
	public partial class ImageObjectConfig
	{
		[Argument(Required = true)]
		public string Path;
		public Millisecond Time = Millisecond.FromSecond(0.2f);
		public float Vague = 0.2f;
		[PathDefine(PathCategory.Rule)]
		public string Rule;
		[SkipArgument]
		public long? AutoOrder;
		[SkipArgument]
		public ICacheHandle<Texture> Texture;
		[SkipArgument]
		public ICacheHandle<Texture> RuleTexture;

		public void PreloadTexture(string prefix, IPreLoader loader)
		{
			if (!string.IsNullOrEmpty(Path))
			{
				loader.Load<Texture>(prefix + Path);
			}
		}

		public void LoadTexture(string prefix, IResourceCache cache)
		{
			if (!string.IsNullOrEmpty(Path))
			{
				Texture = cache.Load<Texture>(prefix + Path);
			}
		}

		public void PreloadRule(PathConfig path, IPreLoader loader)
		{
			PreloadRule(path.GetRoot(PathCategory.Rule), loader);
		}

		public void PreloadRule(string prefix, IPreLoader loader)
		{
			if (!string.IsNullOrEmpty(Rule))
			{
				loader.Load<Texture>(prefix + Path);
			}
		}

		public void LoadRule(PathConfig path, IResourceCache cache)
		{
			LoadRule(path.GetRoot(PathCategory.Rule), cache);
		}

		public void LoadRule(string prefix, IResourceCache cache)
		{
			if (!string.IsNullOrEmpty(Rule))
			{
				RuleTexture = cache.Load<Texture>(prefix + Path);
			}
		}

		public static ImageObjectConfig Restore(ImageObjectEnvData data, string prefix, IResourceCache cache)
		{
			var config = new ImageObjectConfig();
			config.Path = data.Path;
			config.AutoOrder = data.AutoOrder;
			config.LoadTexture(prefix, cache);
			return config;
		}

	}

	public struct ImageObjectEnvData : IEnvDataUpdate<ImageObjectConfig>, IScreenChildEnvData
	{

		public string Path;
		public long AutoOrder;

		public ImageObjectEnvData(ImageObjectConfig conf)
		{
			Path = conf.Path;
			AutoOrder = conf.AutoOrder.GetValueOrDefault();
		}

		public ImageObjectEnvData(string path, int order)
		{
			Path = path;
			AutoOrder = order;
		}

		public void Update(ImageObjectConfig arg)
		{
			Path = arg.Path;
			arg.AutoOrder = AutoOrder = arg.AutoOrder.GetValueOrDefault(AutoOrder);
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