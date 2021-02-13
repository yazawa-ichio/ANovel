using System;
using System.Collections.Generic;

namespace ANovel.Core
{

	public class EventBroker : IDisposable
	{

		int m_Lock = 0;
		bool m_Refresh = false;

		List<Holder> m_Holder = new List<Holder>();

		void CheckDiposed()
		{
			if (m_Holder == null)
			{
				throw new ObjectDisposedException(typeof(EventBroker).FullName);
			}
		}

		public ScopedEventBroker Scoped(object owner = null)
		{
			CheckDiposed();
			var ret = new ScopedEventBroker(this);
			if (owner != null)
			{
				ret.Subscribe(owner);
			}
			return ret;
		}

		public void Publish<T>(T name)
		{
			CheckDiposed();
			var _name = EventNameToStrConverter.ToStr(name);
			try
			{
				m_Lock++;
				// Publish中の追加分にはイベントを飛ばさない
				var count = m_Holder.Count;
				for (int i = 0; i < count; i++)
				{
					m_Holder[i]?.Invoke(_name);
				}
			}
			finally
			{
				m_Lock--;
				if (m_Refresh)
				{
					Refresh();
				}
			}
		}

		public void Publish<T>(T name, object prm)
		{
			CheckDiposed();
			var _name = EventNameToStrConverter.ToStr(name);
			var arg = new object[] { prm };
			try
			{
				m_Lock++;
				// Publish中の追加分にはイベントを飛ばさない
				var count = m_Holder.Count;
				for (int i = 0; i < count; i++)
				{
					m_Holder[i]?.Invoke(_name, arg);
				}
			}
			finally
			{
				m_Lock--;
				if (m_Refresh)
				{
					Refresh();
				}
			}
		}

		public void Register(object obj)
		{
			CheckDiposed();
			foreach (var holer in m_Holder)
			{
				if (holer != null && holer.Owner == obj)
				{
					//登録済み
					return;
				}
			}
			m_Holder.Add(new Holder(obj));
		}

		public void Unregister(object obj)
		{
			if (m_Holder == null) return;

			for (int i = m_Holder.Count - 1; i >= 0; i--)
			{
				var holder = m_Holder[i];
				if (holder != null && holder.Owner == obj)
				{
					if (m_Lock > 0)
					{
						m_Holder[i] = null;
						m_Refresh = true;
					}
					else
					{
						m_Holder.RemoveAt(i);
					}

				}
			}
		}

		public void Refresh()
		{
			if (m_Holder == null) return;

			if (m_Lock > 0)
			{
				m_Refresh = true;
				return;
			}
			m_Refresh = false;
			for (int i = m_Holder.Count - 1; i >= 0; i--)
			{
				var holder = m_Holder[i];
				if (holder == null || holder.Owner == null || holder.Owner.Equals(null))
				{
					m_Holder.RemoveAt(i);
				}
			}
		}

		public void Dispose()
		{
			if (m_Holder != null)
			{
				m_Holder.Clear();
				m_Holder = null;
			}
		}

		class Holder
		{
			object m_Owner;
			EventEntry m_Entry;

			public object Owner => m_Owner;

			public Holder(object owner)
			{
				m_Owner = owner;
				m_Entry = EventEntry.Get(owner.GetType());
			}

			public void Invoke(string name)
			{
				if (!m_Owner.Equals(null))
				{
					m_Entry.Invoke(m_Owner, name);
				}
			}

			public void Invoke(string name, params object[] prm)
			{
				if (!m_Owner.Equals(null))
				{
					m_Entry.Invoke(m_Owner, name, prm);
				}
			}
		}

	}


}