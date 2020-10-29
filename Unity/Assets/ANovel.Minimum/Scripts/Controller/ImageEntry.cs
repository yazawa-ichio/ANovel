using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Minimum
{
	public class ImageEntry
	{
		public readonly string Name;
		public Image Image;
		public Animation Animation;
		public Level Level;
		public CommandCoroutine Show;
		public CommandCoroutine Action;
		int m_Width;
		int m_Height;

		public ImageEntry(string name)
		{
			Name = name;
		}

		public void SetSprite(Sprite sprite)
		{
			Image.sprite = sprite;
			Image.SetNativeSize();
			var size = Image.rectTransform.sizeDelta;
			m_Width = (int)size.x;
			m_Height = (int)size.y;
		}

		public void SetSize(float? x, float? y)
		{
			var size = Image.rectTransform.sizeDelta;
			if (x != null)
			{
				size.x = x.Value * m_Width;
			}
			if (y != null)
			{
				size.y = y.Value * m_Height;
			}
			Image.rectTransform.sizeDelta = size;
		}

		public void SetPos(float? posX, float? posY)
		{
			var pos = Image.rectTransform.anchoredPosition;
			if (posX != null)
			{
				pos.x = posX.Value;
			}
			if (posY != null)
			{
				pos.y = posY.Value;
			}
			Image.rectTransform.anchoredPosition = pos;
		}

		public void SetAlpha(float v)
		{
			var color = Image.color;
			color.a = v;
			Image.color = color;
		}

		public void SetLayout(LayoutConfig config)
		{
			SetSize(config.ScaleX, config.ScaleY);
			SetPos(config.PosX, config.PosY);
		}

		public Animation GetAnimation()
		{
			if (Animation == null)
			{
				Animation = Image.gameObject.AddComponent<Animation>();
			}
			return Animation;
		}

		public LayoutConfig GetLayout()
		{
			var pos = Image.rectTransform.anchoredPosition;
			var size = Image.rectTransform.sizeDelta;
			return new LayoutConfig
			{
				PosX = pos.x,
				PosY = pos.y,
				ScaleX = size.x / m_Width,
				ScaleY = size.y / m_Height,
			};
		}

		public void SetLayout(LayoutConfig prev, LayoutConfig config, float rate)
		{
			var pos = Image.rectTransform.anchoredPosition;
			var size = Image.rectTransform.sizeDelta;
			if (config.PosX != null)
			{
				pos.x = prev.PosX.Value + (config.PosX.Value - prev.PosX.Value) * rate;
			}
			if (config.PosY != null)
			{
				pos.y = prev.PosY.Value + (config.PosY.Value - prev.PosY.Value) * rate;
			}
			if (config.ScaleX != null)
			{
				size.x = (prev.ScaleX.Value + (config.ScaleX.Value - prev.ScaleX.Value) * rate) * m_Width;
			}
			if (config.ScaleY != null)
			{
				size.y = (prev.ScaleY.Value + (config.ScaleY.Value - prev.ScaleY.Value) * rate) * m_Height;
			}
			Image.rectTransform.sizeDelta = size;
			Image.rectTransform.anchoredPosition = pos;
		}
	}
}