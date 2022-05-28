using System;

namespace ANovel.Engine
{
	public partial class ImageController
	{
		ServiceContainer m_Container;
		Image m_Current;
		Image m_Prev;
		IScreenService Screen => m_Container.Get<IScreenService>();

		public ImageController(ServiceContainer container)
		{
			m_Container = container;
		}

		public void OnBeginSwap(BeginSwapEvent e)
		{
			Delete(e.NextId);
			m_Prev = m_Current;
			m_Current = null;
			if (m_Prev != null && e.Copy)
			{
				m_Current = m_Prev.Copy(e.NextId);
			}
		}

		public void Delete(IScreenId id)
		{
			if (m_Current != null && m_Current.Id == id)
			{
				m_Current.Dispose();
				m_Current = null;
			}
			if (m_Prev != null && m_Prev.Id == id)
			{
				m_Prev.Dispose();
				m_Prev = null;
			}
		}

		public IPlayHandle Show(ImageObjectConfig config, LayoutConfig layout)
		{
			if (m_Current == null)
			{
				m_Current = new Image(m_Container, Screen.CurrentId);
			}
			return m_Current.Show(config, layout);
		}

		public IPlayHandle Change(ImageObjectConfig config)
		{
			if (m_Current == null)
			{
				return FloatFadeHandle.Empty;
			}
			return m_Current.Change(config);
		}

		public IPlayHandle Hide(ImageObjectConfig config)
		{
			if (m_Current == null)
			{
				return FloatFadeHandle.Empty;
			}
			return m_Current.Hide(config);
		}

		public IPlayHandle PlayAnim(PlayAnimConfig config, LayoutConfig layout)
		{
			if (m_Current == null)
			{
				throw new Exception($"play target not found ");
			}
			return m_Current.PlayAnim(config, layout);
		}

		public void SetOrder(long autoOrder)
		{
			if (m_Current == null) return;
			m_Current.SetOrder(autoOrder);
		}

	}

}