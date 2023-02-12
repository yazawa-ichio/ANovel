using ANovel.Actions;
using System.Collections.Generic;

namespace ANovel.Engine
{
	public struct ActionLayoutEnvData : IActionParam
	{
		public Millisecond Start { get; set; }
		public Millisecond Time => Config.Time;
		public PlayAnimConfig Config { get; set; }
		public LayoutConfig From { get; set; }
		public LayoutConfig To { get; set; }

		public IEnumerable<IActionPlaying> CreateActions(object target)
		{
			if (target is ImageObject image)
			{
				var from = From.GetLayout(new ImageLayout(), image.TexSize, image.ScreenSize);
				var to = To.GetLayout(from, image.TexSize, image.ScreenSize);
				foreach (var item in from.GetDiff(to))
				{
					yield return new ActionImageParamPlaying(Start, Time, image, item.type, item.from, item.to, Config.Easing);
				}
			}
		}
	}

}