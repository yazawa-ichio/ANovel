using System;

namespace ANovel.Service
{
	public interface IScreenId : IEquatable<IScreenId>
	{
		bool IsCurrent { get; }
	}

	public class ScreenId : IScreenId
	{
		ScreenView m_View;

		public bool IsCurrent => m_View.IsCurrent;

		public ScreenId(ScreenView view)
		{
			m_View = view;
		}

		public override int GetHashCode()
		{
			return m_View.GetHashCode();
		}

		public bool Equals(IScreenId other)
		{
			if (other is ScreenId id)
			{
				return id.m_View == m_View;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is ScreenId id)
			{
				return id.m_View == m_View;
			}
			return ReferenceEquals(this, obj);
		}

	}
}