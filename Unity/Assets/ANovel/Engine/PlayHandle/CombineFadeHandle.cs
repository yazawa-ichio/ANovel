using System;

namespace ANovel
{
	public class CombineFadeHandle : IFadeHandle
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

		IFadeHandle[] m_Handles;
		Action m_OnComplete;

		public CombineFadeHandle(IFadeHandle[] handles)
		{
			m_Handles = handles;
			IsPlaying = m_Handles.Length > 0;
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

		public void Update(float deltaTime)
		{
			if (!IsPlaying)
			{
				return;
			}
			bool isPlaying = false;
			foreach (var handle in m_Handles)
			{
				if (handle.IsPlaying)
				{
					handle.Update(deltaTime);
					if (handle.IsPlaying)
					{
						isPlaying = true;
					}
				}
			}
			if (!isPlaying)
			{
				IsPlaying = false;
				m_OnComplete?.Invoke();
				m_OnComplete = null;
			}
		}
	}

}