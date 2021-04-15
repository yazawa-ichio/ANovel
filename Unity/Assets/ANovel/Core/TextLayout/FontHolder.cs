using UnityEngine;

namespace ANovel.Core
{
	public interface IFontHolder
	{
		string FontName { get; }
		bool GetSize(char ch, int size, out Vector2Int rect, FontStyle style);
	}

	public class FontHolder : IFontHolder
	{
		Font m_Font;

		public string FontName => m_Font.name;

		public FontHolder(Font font) => m_Font = font;

		public bool GetSize(char ch, int fontSize, out Vector2Int glyphSize, FontStyle style)
		{
			if (m_Font.dynamic)
			{
				m_Font.RequestCharactersInTexture(ch.ToString(), fontSize, style);
			}
			if (m_Font.GetCharacterInfo(ch, out var info, fontSize, style))
			{
				glyphSize = new Vector2Int(info.glyphWidth, fontSize);
				return true;
			}
			else
			{
				glyphSize = default;
				return false;
			}
		}

	}

}