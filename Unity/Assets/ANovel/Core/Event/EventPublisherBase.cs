using System.Collections.Generic;

namespace ANovel.Core
{
	public abstract class EventPublisherBase<TEntry, TValue> : IEventPublisher where TEntry : struct, IEvent<TValue>
	{

		int m_Lock;
		bool m_Refresh;
		protected Dictionary<string, List<TEntry>> m_Dic = new Dictionary<string, List<TEntry>>();

		protected void PublishImpl(string name, TValue value)
		{
			try
			{
				m_Lock++;
				if (m_Dic.TryGetValue(name, out var entries))
				{
					// Publish中の追加分にはイベントを飛ばさない
					var count = entries.Count;
					for (int i = 0; i < count; i++)
					{
						var obj = entries[i].Owner;
						if (obj != null && !obj.Equals(null))
						{
							entries[i].Invoke(value);
						}
					}
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

		protected void UnsubscribeImpl(string name, object action)
		{
			if (m_Dic.TryGetValue(name, out var entries))
			{
				for (int i = entries.Count - 1; i >= 0; i--)
				{
					var obj = entries[i].Owner;
					if (obj == null || obj.Equals(null) || entries[i].IsSameAction(action))
					{
						if (m_Lock == 0)
						{
							entries.RemoveAt(i);
						}
						else
						{
							entries[i] = default;
							m_Refresh = true;
						}
					}
				}
			}
		}

		public void Unsubscribe(object owner)
		{
			foreach (var entries in m_Dic.Values)
			{
				for (int i = entries.Count - 1; i >= 0; i--)
				{
					var obj = entries[i].Owner;
					if (obj == owner || obj == null || obj.Equals(null))
					{
						if (m_Lock == 0)
						{
							entries.RemoveAt(i);
						}
						else
						{
							entries[i] = default;
							m_Refresh = true;
						}
					}
				}
			}
		}

		public void Refresh()
		{
			if (m_Lock > 0)
			{
				m_Refresh = true;
				return;
			}
			m_Refresh = false;
			foreach (var value in m_Dic.Values)
			{
				for (int i = value.Count - 1; i >= 0; i--)
				{
					var obj = value[i].Owner;
					if (obj == null || obj.Equals(null))
					{
						value.RemoveAt(i);
					}
				}
			}
		}

	}

}