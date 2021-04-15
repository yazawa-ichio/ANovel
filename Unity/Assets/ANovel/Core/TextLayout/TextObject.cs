using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Core
{
	public class TextObject : MonoBehaviour
	{
		public static TextObject Create(Transform parent)
		{
			var obj = new GameObject(nameof(TextObject));
			obj.transform.SetParent(parent);
			var text = obj.AddComponent<TextObject>();
			text.m_RectTransform = obj.AddComponent<RectTransform>();
			text.m_Text = obj.AddComponent<Text>();
			return text;
		}

		[SerializeField]
		private RectTransform m_RectTransform;
		[SerializeField]
		private Text m_Text;

		public void SetInfo(Font font, ILetterParam layout)
		{
			m_Text.font = font;
			m_Text.fontSize = layout.FontSize;
			m_Text.text = layout.Character;
			m_Text.color = layout.Color;
			m_Text.fontStyle = FontStyle.Normal;
			m_Text.supportRichText = false;
			m_Text.alignment = TextAnchor.MiddleCenter;
			m_Text.horizontalOverflow = HorizontalWrapMode.Overflow;
			m_Text.verticalOverflow = VerticalWrapMode.Overflow;
			m_RectTransform.anchorMin = new Vector2(0, 1);
			m_RectTransform.anchorMax = new Vector2(0, 1);
			m_RectTransform.pivot = new Vector2(0, 1);
			m_RectTransform.anchoredPosition = layout.Pos * new Vector2(1, -1);
			m_RectTransform.sizeDelta = layout.Size;
		}
	}
}