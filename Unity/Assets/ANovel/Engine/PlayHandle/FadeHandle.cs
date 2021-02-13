using System;
using UnityEngine;

namespace ANovel
{
	public class FloatFadeHandle : FadeHandle<float>
	{
		public static readonly FloatFadeHandle Empty = new FloatFadeHandle();

		private FloatFadeHandle() { }

		public FloatFadeHandle(float to, float from, float time) : base(to, from, time)
		{
		}

		protected override float Calc(float from, float to, float rate)
		{
			return from + (to - from) * rate;
		}
	}

	public class Vector2FadeHandle : FadeHandle<Vector2>
	{
		public static readonly Vector2FadeHandle Empty = new Vector2FadeHandle();

		private Vector2FadeHandle() { }

		public Vector2FadeHandle(Vector2 to, Vector2 from, float time) : base(to, from, time) { }

		protected override Vector2 Calc(Vector2 from, Vector2 to, float rate)
		{
			return from + (to - from) * rate;
		}
	}

	public class Vector3FadeHandle : FadeHandle<Vector3>
	{
		public static readonly Vector3FadeHandle Empty = new Vector3FadeHandle();

		private Vector3FadeHandle() { }

		public Vector3FadeHandle(Vector3 to, Vector3 from, float time) : base(to, from, time) { }

		protected override Vector3 Calc(Vector3 from, Vector3 to, float rate)
		{
			return from + (to - from) * rate;
		}
	}

	public class ColorFadeHandle : FadeHandle<Color>
	{
		public static readonly ColorFadeHandle Empty = new ColorFadeHandle();

		private ColorFadeHandle() { }

		public ColorFadeHandle(Color to, Color from, float time) : base(to, from, time) { }

		protected override Color Calc(Color from, Color to, float rate)
		{
			return from + (to - from) * rate;
		}
	}

	public interface IFadeHandle : IPlayHandle
	{
		void Update(float deltaTime);
	}

	public abstract class FadeHandle<T> : IFadeHandle
	{

		T m_From;
		T m_To;
		float m_Time;
		float m_Timer;

		public bool IsPlaying { get; private set; }

		public Func<float, float> Easing { get; set; }

		public Action<T> Output { get; set; }

		public float Time => m_Time;

		public float Timer => m_Timer;

		public T From => m_From;

		public T To => m_To;

		Action m_OnComplete;
		public Action OnComplete
		{
			get => m_OnComplete;
			set
			{
				if (IsPlaying)
				{
					m_OnComplete = value;
				}
				else
				{
					value?.Invoke();
				}
			}
		}

		event Action IPlayHandle.OnComplete
		{
			remove => m_OnComplete -= value;
			add
			{
				if (IsPlaying)
				{
					m_OnComplete += value;
				}
				else
				{
					value?.Invoke();
				}
			}
		}

		public T Value { get; private set; }

		protected abstract T Calc(T from, T to, float rate);

		protected FadeHandle()
		{
			IsPlaying = false;
		}

		public FadeHandle(T to, T from, float time)
		{
			m_To = to;
			m_From = from;
			m_Time = time;
			m_Timer = 0;
			IsPlaying = true;
		}

		public void Update(float deltaTime)
		{
			if (!IsPlaying) return;
			m_Timer += deltaTime;
			if (m_Timer > m_Time)
			{
				Dispose();
			}
			else
			{
				var rate = m_Timer / m_Time;
				if (Easing != null)
				{
					rate = Easing(rate);
				}
				Value = Calc(m_From, m_To, rate);
				Output?.Invoke(Value);
			}
		}

		public void Dispose()
		{
			if (IsPlaying)
			{
				IsPlaying = false;
				Value = m_To;
				Output?.Invoke(m_To);
				Output = null;
				OnComplete?.Invoke();
				OnComplete = null;
			}
		}

	}

}