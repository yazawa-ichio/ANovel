using System;

namespace ANovel
{
	[Serializable]
	public readonly struct Millisecond : IEquatable<Millisecond>
	{

		public static Millisecond FromSecond(int value) => new Millisecond(value * 1000);

		public static Millisecond FromSecond(float value) => new Millisecond((int)(value * 1000));

		public readonly int Value;

		public Millisecond(int millisecond) => Value = millisecond;

		public override string ToString()
		{
			return Value.ToString();
		}

		public bool Equals(Millisecond other)
		{
			return other.Value == Value;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is Millisecond millisecond)
			{
				return millisecond.Value == Value;
			}
			var value = Value / 1000;
			return value.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public float ToSecond()
		{
			return Value / 1000f;
		}

		public Millisecond Add(int millisecond) => new Millisecond(millisecond + Value);

		public Millisecond AddSecond(int second) => new Millisecond(second * 1000 + Value);

		public Millisecond AddSecond(float second) => new Millisecond((int)(second * 1000) + Value);

	}

}