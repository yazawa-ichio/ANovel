using System;

namespace ANovel.Engine
{
	public partial class ImageController
	{
		class Image : IDisposable
		{
			IScreenService Screen => m_Container.Get<IScreenService>();
			ImagePool Pool => m_Container.Get<ImagePool>();

			public IScreenId Id { get; private set; }

			ServiceContainer m_Container;
			ImageObject m_Current;
			ImageObject m_Prev;
			IPlayHandle m_Playing;
			string m_Level;

			public Image(ServiceContainer container, IScreenId id)
			{
				m_Container = container;
				Id = id;
			}

			public IPlayHandle Show(ImageObjectConfig config, LayoutConfig layout)
			{
				m_Playing?.Dispose();
				m_Playing = null;
				m_Level = layout.Level;
				var level = Screen.GetLevel(layout.Level);
				if (Screen.Transition.IsTransition)
				{
					if (m_Current == null)
					{
						m_Current = Pool.Get(level);
					}
					m_Current.SetLayout(layout.GetLayout(m_Current.GetLayout(current: false), config.Texture, Screen.Size));
					m_Current.SetConfig(config);
					return FloatFadeHandle.Empty;
				}
				if (m_Current == null)
				{
					m_Current = Pool.Get(level);
					m_Current.SetLevel(level);
					m_Current.SetLayout(layout.GetLayout(m_Current.GetLayout(), config.Texture, Screen.Size));
					return m_Playing = m_Current.Transition(config);
				}
				var cur = m_Current.GetLayout();
				var next = layout.GetLayout(cur, config.Texture, Screen.Size);
				if (cur.CanTransition(in next))
				{
					m_Current.SetLayout(next);
					return m_Playing = m_Current.Transition(config);
				}
				else
				{
					m_Prev = m_Current;
					m_Current = Pool.Get(level);
					m_Current.SetLayout(next);
					var hide = m_Prev.Transition(new ImageObjectConfig
					{
						Time = config.Time,
						Vague = config.Vague,
						RuleTexture = config.RuleTexture,
						AutoOrder = config.AutoOrder.GetValueOrDefault() + 1,
					});
					hide.OnComplete += () =>
					{
						if (m_Prev != null)
						{
							Pool.Return(m_Prev);
							m_Prev = null;
						}
					};
					var show = m_Current.Transition(config);
					show.OnComplete += () =>
					{
						hide.Dispose();
					};
					return m_Playing = show;
				}
			}

			public IPlayHandle Change(ImageObjectConfig config)
			{
				m_Playing?.Dispose();
				m_Playing = null;
				if (Screen.Transition.IsTransition)
				{
					m_Current.SetConfig(config);
					return FloatFadeHandle.Empty;
				}
				return m_Playing = m_Current.Transition(config);
			}

			public IPlayHandle Hide(ImageObjectConfig config)
			{
				m_Playing?.Dispose();
				m_Playing = null;
				if (Screen.Transition.IsTransition)
				{
					Pool.Return(m_Current);
					m_Current = null;
					return FloatFadeHandle.Empty;
				}
				var hide = m_Current.Transition(config);
				hide.OnComplete += () =>
				{
					Pool.Return(m_Current);
					m_Current = null;
				};
				return m_Playing = hide;
			}

			public IPlayHandle PlayAnim(PlayAnimConfig config, LayoutConfig layout)
			{
				if (m_Current == null)
				{
					return FloatFadeHandle.Empty;
				}
				var cur = m_Current.GetLayout(false);
				var next = layout.GetLayout(cur, m_Current.TexSize, Screen.Size);
				return m_Current.PlayAnim(cur.GetAnims(config.Time, config.Easing, next));
			}

			public void SetOrder(long autoOrder)
			{
				if (m_Current == null) return;
				m_Current.SetOrder(autoOrder);
			}

			public Image Copy(IScreenId dstId)
			{
				m_Playing?.Dispose();
				m_Playing = null;
				var dst = new Image(m_Container, dstId);
				if (m_Current != null)
				{
					var level = Screen.GetLevel(m_Level);
					dst.m_Current = Pool.Get(level);
					dst.m_Current.Copy(m_Current, true);
				}
				return dst;
			}

			public void Dispose()
			{
				m_Playing?.Dispose();
				m_Playing = null;
				if (m_Current != null)
				{
					Pool.Return(m_Current);
					m_Current = null;
				}
				if (m_Prev != null)
				{
					Pool.Return(m_Prev);
					m_Prev = null;
				}
			}
		}
	}
}