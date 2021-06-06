using UnityEngine;

namespace ANovel.Engine
{
	public class ScreenTransitionConfig
	{
		public Millisecond Time = Millisecond.FromSecond(0.5f);
		public float Vague = 0.2f;
		[Description("現在の画面をキャプチャーして切り替えます")]
		public bool Capture = true;
		[Description("現在の情報を切り替え先にコピーします")]
		public bool Copy = true;
		[SkipArgument]
		public ICacheHandle<Texture> Rule;
	}
}