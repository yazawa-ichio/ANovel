using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Minimum
{
	public interface IImageController : IController
	{
		CommandCoroutine ShowImage(ShowConfig config, LayoutConfig layout);
		CommandCoroutine HideImage(HideConfig config);
		CommandCoroutine AllHideImage(AllHideConfig config);
		void LayoutImage(LayoutConfig config);
		CommandCoroutine PlayAnimImage(AnimationConfig config);
		CommandCoroutine PlayMoveImage(LayoutConfig config, float time);
		void StopActionImage(string name);
	}

	public class ShowConfig
	{
		[CommandField(Required = true)]
		public string Name;
		[NonSerialized]
		public Sprite Sprite;
		public Level Level = Level.Middle;
		public float Time = 0.2f;
	}

	public class LayoutConfig
	{
		[CommandField(Required = true)]
		public string Name;
		[CommandField(KeyName = "x")]
		public float? PosX;
		[CommandField(KeyName = "y")]
		public float? PosY;
		public float? ScaleX;
		public float? ScaleY;
	}

	public class HideConfig
	{
		[CommandField(Required = true)]
		public string Name;
		public float Time = 0.2f;
	}

	public class AllHideConfig
	{
		public Level? Level;
		public float Time = 0.2f;
	}

	public class AnimationConfig
	{
		[CommandField(Required = true)]
		public string Name;
		[NonSerialized]
		public AnimationClip Clip;
		public float Speed = 1f;
	}

	public class ImageController : ControllerBase, IImageController
	{

		public override Type ControllerType => typeof(IImageController);

		[SerializeField]
		RectTransform m_BG = default;
		[SerializeField]
		RectTransform m_Back = default;
		[SerializeField]
		RectTransform m_Middle = default;
		[SerializeField]
		RectTransform m_Front = default;
		[SerializeField]
		RectTransform m_Over = default;

		Dictionary<Level, RectTransform> m_LevelDic = new Dictionary<Level, RectTransform>();
		Dictionary<string, ImageEntry> m_Images = new Dictionary<string, ImageEntry>();
		Queue<Image> m_Pool = new Queue<Image>();

		void Awake()
		{
			m_LevelDic[Level.BG] = m_BG;
			m_LevelDic[Level.Back] = m_Back;
			m_LevelDic[Level.Middle] = m_Middle;
			m_LevelDic[Level.Front] = m_Front;
			m_LevelDic[Level.Over] = m_Over;
		}

		Image Get(RectTransform level)
		{
			Image image;
			if (m_Pool.Count > 0)
			{
				image = m_Pool.Dequeue();
				image.gameObject.SetActive(true);
			}
			else
			{
				GameObject obj = new GameObject("Image");
				image = obj.AddComponent<Image>();
			}
			image.rectTransform.SetParent(level);
			image.transform.localScale = Vector3.one;
			image.transform.localPosition = Vector3.zero;
			return image;
		}

		void Return(Image image)
		{
			image.sprite = null;
			image.gameObject.SetActive(false);
			m_Pool.Enqueue(image);
		}

		public CommandCoroutine ShowImage(ShowConfig config, LayoutConfig layout)
		{
			if (m_Images.TryGetValue(config.Name, out var entry))
			{
				entry.Show?.Finish();
				entry.Show = null;
				entry.Action?.Finish();
				entry.Action = null;
			}
			var level = m_LevelDic[config.Level];
			if (config.Time <= 0)
			{
				if (entry != null)
				{
					entry.Image.transform.SetParent(level);
				}
				else
				{
					m_Images[config.Name] = entry = new ImageEntry(config.Name) { Image = Get(level) };
				}
				entry.SetSprite(config.Sprite);
				entry.Level = config.Level;
				entry.SetAlpha(1f);
				entry.SetLayout(layout);
				return CommandCoroutine.Empty;
			}
			else
			{
				Image prev = null;
				if (entry != null)
				{
					prev = entry.Image;
					entry.Image = Get(level);
				}
				else
				{
					m_Images[config.Name] = entry = new ImageEntry(config.Name) { Image = Get(level) };
				}
				entry.SetSprite(config.Sprite);
				entry.Level = config.Level;
				entry.SetLayout(layout);
				return new CommandCoroutine(this, Show(entry, config, prev), () =>
				{
					entry.SetAlpha(1f);
					if (prev != null)
					{
						Return(prev);
					}
				});
			}
		}

		IEnumerator Show(ImageEntry entry, ShowConfig config, Image prev)
		{
			float timer = 0;
			entry.SetAlpha(0);
			while (timer < config.Time)
			{
				timer += DeltaTime;
				var a = timer / config.Time;
				entry.SetAlpha(a);
				if (prev != null)
				{
					var color = prev.color;
					color.a = 1f - a;
					prev.color = color;
				}
				yield return null;
			}
		}

		public CommandCoroutine HideImage(HideConfig config)
		{
			if (m_Images.TryGetValue(config.Name, out var entry))
			{
				m_Images.Remove(config.Name);
				entry.Show?.Finish();
				entry.Show = null;
				if (config.Time <= 0)
				{
					Return(entry.Image);
				}
				else
				{
					return new CommandCoroutine(this, Hide(entry, config), () =>
					{
						Return(entry.Image);
					});
				}
			}
			return CommandCoroutine.Empty;
		}

		IEnumerator Hide(ImageEntry entry, HideConfig config)
		{
			float timer = 0;
			while (timer < config.Time)
			{
				timer += DeltaTime;
				entry.SetAlpha(1f - timer / config.Time);
				yield return null;
			}
		}

		public CommandCoroutine AllHideImage(AllHideConfig config)
		{
			ImageEntry[] entries;
			if (config.Level != null)
			{
				List<ImageEntry> list = new List<ImageEntry>();
				foreach (var entry in m_Images.Values)
				{
					if (entry.Level == config.Level)
					{
						list.Add(entry);
					}
				}
				entries = list.ToArray();
				foreach (var entry in list)
				{
					m_Images.Remove(entry.Name);
				}
			}
			else
			{
				entries = m_Images.Values.ToArray();
				m_Images.Clear();
			}
			foreach (var entry in entries)
			{
				entry.Show?.Finish();
				entry.Show = null;
			}
			return new CommandCoroutine(this, AllHide(entries, config), () =>
			{
				foreach (var entry in entries)
				{
					Return(entry.Image);
				}
			});
		}

		IEnumerator AllHide(ImageEntry[] entries, AllHideConfig config)
		{
			float timer = 0;
			while (timer < config.Time)
			{
				timer += DeltaTime;
				var a = 1f - timer / config.Time;
				foreach (var entry in entries)
				{
					entry.SetAlpha(a);
				}
				yield return null;
			}
		}


		public void LayoutImage(LayoutConfig config)
		{
			if (m_Images.TryGetValue(config.Name, out var entry))
			{
				entry.SetLayout(config);
			}
		}

		public CommandCoroutine PlayAnimImage(AnimationConfig config)
		{
			if (m_Images.TryGetValue(config.Name, out var entry))
			{
				entry.Action?.Finish();
				var anim = entry.Image.GetComponent<Animation>();
				if (anim == null)
				{
					anim = entry.Image.gameObject.AddComponent<Animation>();
				}
				return entry.Action = new CommandCoroutine(this, PlayAnim(anim, config), () =>
				{
					anim.Stop();
					anim.clip = null;
				});
			}
			return CommandCoroutine.Empty;
		}

		IEnumerator PlayAnim(Animation anim, AnimationConfig config)
		{
			anim.clip = config.Clip;
			while (anim != null && anim.isPlaying)
			{
				yield return null;
			}
		}

		public void StopActionImage(string name)
		{
			if (m_Images.TryGetValue(name, out var entry))
			{
				entry.Action?.Finish();
				entry.Action = null;
			}
		}

		public CommandCoroutine PlayMoveImage(LayoutConfig config, float time)
		{
			if (m_Images.TryGetValue(config.Name, out var entry))
			{
				entry.Action?.Finish();
				return entry.Action = new CommandCoroutine(this, PlayMove(entry, config, time), () =>
				{
					entry.SetLayout(config);
				});
			}
			return CommandCoroutine.Empty;
		}

		IEnumerator PlayMove(ImageEntry entry, LayoutConfig config, float time)
		{
			float timer = 0;
			var prev = entry.GetLayout();
			while (timer < time)
			{
				timer += DeltaTime;
				var rate = timer / time;
				entry.SetLayout(prev, config, rate);
				yield return null;
			}
		}

		public override void Clear()
		{
			foreach (var key in m_Images.Keys.ToArray())
			{
				if (m_Images.TryGetValue(key, out var entry))
				{
					m_Images.Remove(key);
					Return(entry.Image);
					entry.Show?.Finish();
					entry.Show = null;
					entry.Action?.Finish();
					entry.Action = null;
				}
			}
		}


	}
}