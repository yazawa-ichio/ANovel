using ANovel.Core;
using UnityEngine;

namespace ANovel.Service
{
	public class ScreenTransitionConfig
	{
		public Millisecond Time = Millisecond.FromSecond(0.5f);
		public float Vague = 0.2f;
		public bool Capture = true;
		public bool Copy = true;
		[SkipInjectParam]
		public ICacheHandle<Texture> Rule;
	}
}