using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel
{
	public class CombinePlayHandle : IPlayHandle
	{
		public bool IsPlaying { get; private set; }

		public event Action OnComplete
		{
			add
			{
				if (!IsPlaying)
				{
					value?.Invoke();
				}
				else
				{
					m_OnComplete += value;
				}
			}
			remove => m_OnComplete -= value;
		}

		IPlayHandle[] m_Handles;
		Action m_OnComplete;
		int m_CompleteCount;

		public CombinePlayHandle(IEnumerable<IPlayHandle> handles) : this(handles.ToArray()) { }

		public CombinePlayHandle(IPlayHandle[] handles)
		{
			m_Handles = handles;
			if (m_Handles.Length > 0)
			{
				IsPlaying = true;
				foreach (var handle in m_Handles)
				{
					handle.OnComplete += OnHandleComplete;
				}
			}
			else
			{
				IsPlaying = false;
			}
		}

		public void Dispose()
		{
			foreach (var handle in m_Handles)
			{
				if (handle.IsPlaying)
				{
					handle.Dispose();
				}
			}
		}

		void OnHandleComplete()
		{
			m_CompleteCount++;
			if (m_CompleteCount == m_Handles.Length)
			{
				IsPlaying = false;
				m_OnComplete?.Invoke();
			}
		}

	}

}