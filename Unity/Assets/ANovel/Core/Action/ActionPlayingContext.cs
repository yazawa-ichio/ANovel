using ANovel.Actions;
using System;
using System.Collections.Generic;

namespace ANovel
{
	public interface IActionData
	{
		IEnumerable<IActionParam> GetParams();
	}

	public class ActionPlayingHandle : IDisposable
	{

		public static readonly ActionPlayingHandle Empty = new ActionPlayingHandle();

		IActionData m_Data;
		Queue<IActionPlaying> m_Temp = new Queue<IActionPlaying>();
		List<IActionPlaying> m_List = new List<IActionPlaying>();
		List<IActionPlaying> m_Playing = new List<IActionPlaying>();
		float m_Time;
		bool m_Complete;

		public float Time => m_Time;

		private ActionPlayingHandle()
		{
			m_Complete = true;
		}

		public ActionPlayingHandle(IActionData data, object target)
		{
			m_Data = data;
			foreach (var param in data.GetParams())
			{
				foreach (var action in param.CreateActions(target))
				{
					m_List.Add(action);
				}
			}
		}

		public bool IsPlaying => !m_Complete;

		public event Action OnComplete;

		public ActionPlayingHandle Copy(object target)
		{
			var ctx = new ActionPlayingHandle(m_Data, target);
			ctx.m_Complete = m_Complete;
			ctx.Update(m_Time);
			return ctx;
		}

		public void Update(float deltaTime)
		{
			if (m_Complete)
			{
				return;
			}
			m_Time += deltaTime;
			foreach (var item in m_Playing)
			{
				item.Update(deltaTime);
				if (item.Start.Add(item.Time).ToSecond() < m_Time)
				{
					item.End();
					m_Temp.Enqueue(item);
				}
			}
			while (m_Temp.Count > 0)
			{
				m_Playing.Remove(m_Temp.Dequeue());
			}
			foreach (var item in m_List)
			{
				if (item.Start.ToSecond() <= m_Time)
				{

					item.Begin();
					item.Update(m_Time - item.Start.ToSecond());
					m_Temp.Enqueue(item);
				}
			}
			while (m_Temp.Count > 0)
			{
				var item = m_Temp.Dequeue();
				m_List.Remove(item);
				if (item.Start.Add(item.Time).ToSecond() >= m_Time)
				{
					m_Playing.Add(item);
				}
				else
				{
					item.End();
				}
			}
			if (!m_Complete && m_List.Count == 0 && m_Playing.Count == 0)
			{
				m_Complete = true;
				OnComplete?.Invoke();
			}
		}

		public void Dispose()
		{
			foreach (var item in m_List)
			{
				item.Begin();
				m_Playing.Add(item);
			}
			foreach (var item in m_Playing)
			{
				item.End();
			}
			m_Playing.Clear();
			m_List.Clear();
		}
	}
}