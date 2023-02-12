using ANovel.Actions;

namespace ANovel.Engine
{
	public class ActionImageParamPlaying : IActionPlaying
	{
		ImageParamType m_Type;
		ImageObject m_Image;
		FloatFadeHandle m_FadeHandle;

		public Millisecond Time { get; private set; }

		public Millisecond Start { get; private set; }

		public ActionImageParamPlaying(Millisecond start, Millisecond time, ImageObject image, ImageParamType type, float startValue, float endValue, Easing? easing)
		{
			Start = start;
			Time = time;
			m_Type = type;
			m_Image = image;
			m_FadeHandle = new FloatFadeHandle(endValue, startValue, time.ToSecond())
			{
				Easing = easing?.GetMethod(),
				Output = (v) => Set(v),
			};
		}

		void Set(float v)
		{
			m_Image.SetParam(m_Type, v);
		}

		public void Update(float deltaTime)
		{
			m_FadeHandle.Update(deltaTime);
		}

		public void Begin()
		{
			m_FadeHandle.Update(0);
		}

		public void End()
		{
			m_FadeHandle.Dispose();
		}
	}
}