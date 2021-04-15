using UnityEngine;

namespace ANovel.Core
{
	[System.Serializable]
	public struct Margin
	{
		public int Left;
		public int Right;
		public int Top;
		public int Bottom;

		public Margin(int left, int right, int top, int bottom)
		{
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
		}
	}

	public class JText : MonoBehaviour
	{
		[SerializeField]
		Font m_Font;
		[SerializeField]
		int m_FontSize = 24;
		[SerializeField]
		int m_Pitch = 1;
		[SerializeField]
		Margin m_Margin = new Margin(8, 8, 8, 8);
		[SerializeField, TextArea]
		string m_Text;

		public string Text
		{
			get => m_Text;
			set => SetText(value);
		}

		void SetText(string text)
		{
			m_Text = text;
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			var layout = new LetterGenerator((transform as RectTransform).rect, m_Font);
			layout.Margin = m_Margin;
			layout.DefaultParam.FontSize = m_FontSize;
			layout.DefaultParam.Pitch = m_Pitch;
			foreach (var ch in text)
			{
				layout.Add(ch);
			}
			foreach (var obj in gameObject.GetComponentsInChildren<TextObject>(true))
			{
				DestroyImmediate(obj.gameObject);
			}
			foreach (var letter in layout.GetLetters())
			{
				Debug.Log(letter);
				var to = TextObject.Create(transform);
				to.SetInfo(m_Font, letter);
			}
		}

		void OnValidate()
		{
			UnityEditor.EditorApplication.delayCall += () =>
			{
				SetText(m_Text);
			};
		}

	}

}