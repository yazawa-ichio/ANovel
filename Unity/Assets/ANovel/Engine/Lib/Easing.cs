using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANovel
{
	public enum Easing
	{
		Linear,
		QuadraticEaseIn,
		QuadraticEaseOut,
		QuadraticEaseInOut,
		CubicEaseIn,
		CubicEaseOut,
		CubicEaseInOut,
		QuarticEaseIn,
		QuarticEaseOut,
		QuarticEaseInOut,
		QuinticEaseIn,
		QuinticEaseOut,
		QuinticEaseInOut,
		SineEaseIn,
		SineEaseOut,
		SineEaseInOut,
		CircularEaseIn,
		CircularEaseOut,
		CircularEaseInOut,
		ExponentialEaseIn,
		ExponentialEaseOut,
		ExponentialEaseInOut,
		ElasticEaseIn,
		ElasticEaseOut,
		ElasticEaseInOut,
		BackEaseIn,
		BackEaseOut,
		BackEaseInOut,
		BounceEaseIn,
		BounceEaseOut,
		BounceEaseInOut
	}

	public static class EasingMethod
	{
		const float PI = Mathf.PI;

		const float HALFPI = Mathf.PI / 2.0f;

		static Dictionary<Easing, Func<float, float>> m_Cache = new Dictionary<Easing, Func<float, float>>()
		{
			{ Easing.Linear, Linear },
			{ Easing.QuadraticEaseIn, QuadraticEaseIn },
			{ Easing.QuadraticEaseOut, QuadraticEaseOut },
			{ Easing.QuadraticEaseInOut, QuadraticEaseInOut },
			{ Easing.CubicEaseIn, CubicEaseIn },
			{ Easing.CubicEaseOut, CubicEaseOut },
			{ Easing.CubicEaseInOut, CubicEaseInOut },
			{ Easing.QuarticEaseIn, QuarticEaseIn },
			{ Easing.QuarticEaseOut, QuarticEaseOut },
			{ Easing.QuarticEaseInOut, QuarticEaseInOut },
			{ Easing.QuinticEaseIn, QuinticEaseIn },
			{ Easing.QuinticEaseOut, QuinticEaseOut },
			{ Easing.QuinticEaseInOut, QuinticEaseInOut },
			{ Easing.SineEaseIn, SineEaseIn },
			{ Easing.SineEaseOut, SineEaseOut },
			{ Easing.SineEaseInOut, SineEaseInOut },
			{ Easing.CircularEaseIn, CircularEaseIn },
			{ Easing.CircularEaseOut, CircularEaseOut },
			{ Easing.CircularEaseInOut, CircularEaseInOut },
			{ Easing.ExponentialEaseIn, ExponentialEaseIn },
			{ Easing.ExponentialEaseOut, ExponentialEaseOut },
			{ Easing.ExponentialEaseInOut,ExponentialEaseInOut },
			{ Easing.ElasticEaseIn, ElasticEaseIn },
			{ Easing.ElasticEaseOut, ElasticEaseOut },
			{ Easing.ElasticEaseInOut, ElasticEaseInOut },
			{ Easing.BackEaseIn, BackEaseIn },
			{ Easing.BackEaseOut, BackEaseOut },
			{ Easing.BackEaseInOut, BackEaseInOut },
			{ Easing.BounceEaseIn, BounceEaseIn },
			{ Easing.BounceEaseOut, BounceEaseOut },
			{ Easing.BounceEaseInOut, BounceEaseInOut }
		};

		public static Func<float, float> GetMethod(this Easing easing)
		{
			return Get(easing);
		}

		public static Func<float, float> GetMethodOrNull(this Easing? easing)
		{
			if (easing.HasValue)
			{
				return Get(easing.Value);
			}
			return null;
		}

		public static Func<float, float> Get(Easing easing)
		{
			if (m_Cache.TryGetValue(easing, out var ret))
			{
				return ret;
			}
			return m_Cache[Easing.Linear];
		}

		public static float Linear(float p)
		{
			return p;
		}

		public static float QuadraticEaseIn(float p)
		{
			return p * p;
		}

		public static float QuadraticEaseOut(float p)
		{
			return -(p * (p - 2));
		}

		public static float QuadraticEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 2 * p * p;
			}
			else
			{
				return (-2 * p * p) + (4 * p) - 1;
			}
		}

		public static float CubicEaseIn(float p)
		{
			return p * p * p;
		}

		public static float CubicEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f + 1;
		}

		public static float CubicEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 4 * p * p * p;
			}
			else
			{
				float f = ((2 * p) - 2);
				return 0.5f * f * f * f + 1;
			}
		}

		public static float QuarticEaseIn(float p)
		{
			return p * p * p * p;
		}

		public static float QuarticEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f * (1 - p) + 1;
		}

		public static float QuarticEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 8 * p * p * p * p;
			}
			else
			{
				float f = (p - 1);
				return -8 * f * f * f * f + 1;
			}
		}

		public static float QuinticEaseIn(float p)
		{
			return p * p * p * p * p;
		}

		public static float QuinticEaseOut(float p)
		{
			float f = (p - 1);
			return f * f * f * f * f + 1;
		}

		public static float QuinticEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 16 * p * p * p * p * p;
			}
			else
			{
				float f = ((2 * p) - 2);
				return 0.5f * f * f * f * f * f + 1;
			}
		}

		public static float SineEaseIn(float p)
		{
			return Mathf.Sin((p - 1) * HALFPI) + 1;
		}

		public static float SineEaseOut(float p)
		{
			return Mathf.Sin(p * HALFPI);
		}

		public static float SineEaseInOut(float p)
		{
			return 0.5f * (1 - Mathf.Cos(p * PI));
		}

		public static float CircularEaseIn(float p)
		{
			return 1 - Mathf.Sqrt(1 - (p * p));
		}

		public static float CircularEaseOut(float p)
		{
			return Mathf.Sqrt((2 - p) * p);
		}

		public static float CircularEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (p * p)));
			}
			else
			{
				return 0.5f * (Mathf.Sqrt(-((2 * p) - 3) * ((2 * p) - 1)) + 1);
			}
		}

		public static float ExponentialEaseIn(float p)
		{
			return (p == 0.0f) ? p : Mathf.Pow(2, 10 * (p - 1));
		}

		public static float ExponentialEaseOut(float p)
		{
			return (p == 1.0f) ? p : 1 - Mathf.Pow(2, -10 * p);
		}

		public static float ExponentialEaseInOut(float p)
		{
			if (p == 0.0 || p == 1.0) return p;

			if (p < 0.5f)
			{
				return 0.5f * Mathf.Pow(2, (20 * p) - 10);
			}
			else
			{
				return -0.5f * Mathf.Pow(2, (-20 * p) + 10) + 1;
			}
		}

		public static float ElasticEaseIn(float p)
		{
			return Mathf.Sin(13 * HALFPI * p) * Mathf.Pow(2, 10 * (p - 1));
		}

		public static float ElasticEaseOut(float p)
		{
			return Mathf.Sin(-13 * HALFPI * (p + 1)) * Mathf.Pow(2, -10 * p) + 1;
		}

		public static float ElasticEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * Mathf.Sin(13 * HALFPI * (2 * p)) * Mathf.Pow(2, 10 * ((2 * p) - 1));
			}
			else
			{
				return 0.5f * (Mathf.Sin(-13 * HALFPI * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
			}
		}

		public static float BackEaseIn(float p)
		{
			return p * p * p - p * Mathf.Sin(p * PI);
		}

		public static float BackEaseOut(float p)
		{
			float f = (1 - p);
			return 1 - (f * f * f - f * Mathf.Sin(f * PI));
		}

		public static float BackEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				float f = 2 * p;
				return 0.5f * (f * f * f - f * Mathf.Sin(f * PI));
			}
			else
			{
				float f = (1 - (2 * p - 1));
				return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * PI))) + 0.5f;
			}
		}

		public static float BounceEaseIn(float p)
		{
			return 1 - BounceEaseOut(1 - p);
		}

		public static float BounceEaseOut(float p)
		{
			if (p < 4 / 11.0f)
			{
				return (121 * p * p) / 16.0f;
			}
			else if (p < 8 / 11.0f)
			{
				return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
			}
			else if (p < 9 / 10.0f)
			{
				return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
			}
			else
			{
				return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
			}
		}

		public static float BounceEaseInOut(float p)
		{
			if (p < 0.5f)
			{
				return 0.5f * BounceEaseIn(p * 2);
			}
			else
			{
				return 0.5f * BounceEaseOut(p * 2 - 1) + 0.5f;
			}
		}


	}

}