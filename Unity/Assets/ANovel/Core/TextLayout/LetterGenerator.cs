using System.Collections.Generic;
using UnityEngine;

namespace ANovel.Core
{

	public enum LineAlign
	{
		Top,
		Center,
		Bottom,
		Right = Top,
		Left = Bottom,
	}

	public enum TextAlign
	{
		LeftTop,
		Top,
		RightTop,
		Right,
		RightBottom,
		Bottom,
		LeftBottom,
		Left,
		Center,
	}

	public interface ILetterParam
	{
		string Character { get; }
		Vector2 Pos { get; }
		Vector2 Size { get; }
		string FontName { get; }
		int FontSize { get; }
		float LineSize { get; }
		LineAlign LineAlign { get; }
		Color Color { get; }
		IEnumerable<IRubyLetterParam> GetRubys();
	}

	public interface IRubyLetterParam
	{
		string Character { get; }
		float RelativePos { get; }
		Vector2 Size { get; }
		string FontName { get; }
		int FontSize { get; }
	}

	public class LetterGeneratorParam
	{
		public LineAlign LineAlign { get; set; } = LineAlign.Bottom;

		public int FontSize { get; set; } = 24;

		public int RubyFontSize { get; set; } = 12;

		public int Pitch { get; set; }

		public int LineSpace { get; set; }

		public Color Color { get; set; } = Color.white;

		public void Copy(LetterGeneratorParam src)
		{
			LineAlign = src.LineAlign;
			FontSize = src.FontSize;
			RubyFontSize = src.RubyFontSize;
			Pitch = src.Pitch;
			LineSpace = src.LineSpace;
			Color = src.Color;
		}

		public LetterGeneratorParam Clone()
		{
			var ret = new LetterGeneratorParam();
			ret.Copy(this);
			return ret;
		}

	}

	public class LetterGenerator
	{
		public const string DefaultFollowing = "%),:;]}｡｣ﾞﾟ。，、．：；゛゜ヽヾゝゞ々’”）〕］｝〉》」』】°′″℃￠％‰";
		public const string DefaultFollowingWeak = "!.?､･ｧｨｩｪｫｬｭｮｯｰ・？！ーぁぃぅぇぉっゃゅょゎァィゥェォッャュョヮヵヶ";
		public const string DefaultLeading = "\\$([{｢‘“（〔［｛〈《「『【￥＄￡";


		// 公開するか検討中
		bool Vertical { get; set; } = false;

		public Rect Size { get; set; }

		public Margin Margin { get; set; } = new Margin(8, 8, 8, 8);

		public float MarginRCh { get; set; } = 1f;

		public LetterGeneratorParam DefaultParam { get; private set; } = new LetterGeneratorParam();

		public LetterGeneratorParam Param { get; private set; } = new LetterGeneratorParam();

		public string Following { get; set; } = DefaultFollowing;

		public string FollowingWeak { get; set; } = DefaultFollowingWeak;

		public string Leading { get; set; } = DefaultLeading;

		float m_CurrentX;
		float m_CurrentY;
		float m_LineSize;
		float m_IndentPos;
		char m_LastCh;
		List<LetterParam> m_Letters = new List<LetterParam>();
		bool m_Process;
		float m_RelinePos;
		int m_LineId;
		bool m_NewLine;
		IFontHolder m_FontHolder;
		List<LetterParam> m_RubyTargetLetters = new List<LetterParam>();

		public LetterGenerator(Rect size, Font font)
		{
			Size = size;
			m_FontHolder = new FontHolder(font);
		}

		public LetterGenerator(Rect size, IFontHolder font)
		{
			Size = size;
			m_FontHolder = font;
		}

		public void Reset()
		{
			m_Process = false;
			m_NewLine = true;
			m_LineId = 0;
			Param.Copy(DefaultParam);
			if (!Vertical)
			{
				m_RelinePos = Size.width - Margin.Right - MarginRCh * Param.FontSize;
				m_CurrentX = Margin.Left;
				m_CurrentY = Margin.Top;
			}
			else
			{
				m_RelinePos = Size.height - Margin.Bottom - MarginRCh * Param.FontSize;
				m_CurrentX = Size.width - Margin.Right;
				m_CurrentY = Margin.Top;
			}
			m_LastCh = default;
		}

		public void StartIndent()
		{
			m_IndentPos = m_CurrentX - Margin.Left;
		}

		public void EndIndent()
		{
			m_IndentPos = 0;
		}

		// 括弧などを字下げして表示する
		public void BackAndStartIndent(char ch)
		{
			if (!m_NewLine) throw new System.Exception("not newline");
			m_FontHolder.GetSize(ch, Param.FontSize, out var size, FontStyle.Normal);
			m_CurrentX -= Param.Pitch + (Vertical ? size.y : size.x);
			Add(ch);
			StartIndent();
		}

		public void Add(char ch)
		{
			AddImpl(ch);
		}

		public void Add(string text)
		{
			AddImpl(text, null);
		}

		public void Add(string text, string ruby)
		{
			var letters = m_RubyTargetLetters;
			try
			{
				AddImpl(text, letters);

				if (letters.Count == 0)
				{
					throw new System.ArgumentException("ルビが有効なテキストではありません", nameof(text));
				}

				SetRuby(letters, ruby);
			}
			finally
			{
				letters.Clear();
			}
		}

		public IEnumerable<ILetterParam> GetLetters()
		{
			return m_Letters;
		}

		void TryStart()
		{
			if (m_Process) return;
			Reset();
			m_Process = true;
		}

		void AddImpl(string text, List<LetterParam> letters)
		{
			foreach (var ch in text)
			{
				var ret = AddImpl(ch);
				if (ret != null)
				{
					letters?.Add(ret);
				}
			}
		}

		LetterParam AddImpl(char ch)
		{
			TryStart();
			if (ch == '\n')
			{
				ReLine();
				return null;
			}
			var pos = Vertical ? m_CurrentY : m_CurrentX;
			var max = Vertical ? Size.height : Size.width;
			if (pos >= m_RelinePos)
			{
				if (((m_LastCh == default || DefaultLeading.IndexOf(m_LastCh) == -1) &&
					DefaultFollowing.IndexOf(ch) == -1 && DefaultFollowingWeak.IndexOf(ch) == -1) ||
					((m_LastCh != default || DefaultFollowingWeak.IndexOf(m_LastCh) != -1) &&
					DefaultFollowingWeak.IndexOf(ch) != -1))
				{
					ReLine();
				}
				else if (pos >= max)
				{
					ReLine();
				}
			}
			m_LastCh = ch;
			m_NewLine = false;
			if (m_FontHolder.GetSize(ch, Param.FontSize, out var size, FontStyle.Normal))
			{
				var letter = new LetterParam(m_LineId, ch.ToString(), new Vector2(m_CurrentX, m_CurrentY), size, m_FontHolder.FontName, Param);
				m_Letters.Add(letter);
				var lineSize = Vertical ? size.y : size.x;
				if (m_LineSize <= lineSize)
				{
					m_LineSize = lineSize;
				}
				if (!Vertical)
				{
					m_CurrentX += size.x + Param.Pitch;
				}
				else
				{
					m_CurrentY += size.y + Param.Pitch;
				}
				return letter;
			}
			else
			{
				return null;
			}
		}


		void SetRuby(List<LetterParam> letters, string ruby)
		{
			// 文字のサイズなどから範囲とルビのピッチを取得（最小実装なのでピッチは固定）
			GetRubyParam(letters, ruby, out var rubyPitch, out var offsetStart);

			int index = 0;
			var currentPos = letters[index].GetPos(Vertical) + offsetStart;
			foreach (var ch in ruby)
			{
				var cur = letters[index];
				m_FontHolder.GetSize(ch, Param.RubyFontSize, out var size, FontStyle.Normal);
				cur.AddRuby(ch.ToString(), currentPos - cur.GetPos(Vertical), size, m_FontHolder.FontName, Param.RubyFontSize);

				currentPos += Vertical ? size.y : size.x;
				currentPos += rubyPitch;

				var endPos = cur.GetPos(Vertical);
				if (endPos < currentPos && index + 1 < letters.Count)
				{
					index++;
					var next = letters[index];
					if (next.LineId != cur.LineId)
					{
						var offset = currentPos - endPos;
						currentPos = next.GetPos(Vertical) + offset;
					}
				}

			}

		}

		void GetRubyParam(List<LetterParam> letters, string ruby, out float rubyPitch, out float offsetStart)
		{
			var start = letters[0];
			var end = letters[letters.Count - 1];

			// 文字のサイズなどから範囲とルビのピッチを取得（最小実装なのでピッチは固定）
			var totalSize = GetSize(m_RubyTargetLetters);
			var baseSize = totalSize - start.GetSize(Vertical) / 2f - end.GetSize(Vertical) / 2f;
			var rubySize = GetTextSize(ruby, Param.RubyFontSize);
			offsetStart = start.GetSize(Vertical) / 2f;
			rubyPitch = 0;
			if (baseSize < rubySize)
			{
				// 開始と終了の文字に収まらない場合
				offsetStart = -(rubySize - totalSize) / 2f;
			}
			else
			{
				// 開始と終了の文字に収まる場合
				rubyPitch = (baseSize - rubySize) / ruby.Length;
			}
		}

		float GetSize(List<LetterParam> letters)
		{
			if (letters.Count == 0)
			{
				return 0;
			}
			float totalSize = 0;
			var prev = letters[0];
			float start = Vertical ? prev.Pos.y : prev.Pos.x;
			for (int i = 1; i < letters.Count; i++)
			{
				var letter = letters[i];
				if (prev.LineId != letter.LineId)
				{
					var end = Vertical ? (prev.Pos.y + prev.Size.y) : (prev.Pos.x + prev.Size.x);
					totalSize += end - start;
					start = Vertical ? letter.Pos.y : letter.Pos.x;
				}
				prev = letter;
			}
			{
				var end = Vertical ? (prev.Pos.y + prev.Size.y) : (prev.Pos.x + prev.Size.x);
				totalSize += end - start;
			}
			return totalSize;
		}

		float GetTextSize(string text, int fontSize)
		{
			float totalSize = 0;
			foreach (var ch in text)
			{
				m_FontHolder.GetSize(ch, fontSize, out var size, FontStyle.Normal);
				totalSize += Vertical ? size.y : size.x;
			}
			return totalSize;
		}

		public void ReLine()
		{
			m_LastCh = default;
			m_NewLine = true;
			if (m_LineSize == 0)
			{
				m_LineSize = Param.FontSize;
			}
			if (!Vertical)
			{
				m_CurrentX = Margin.Left + m_IndentPos;
				m_CurrentY += m_LineSize + Param.LineSpace;
			}
			else
			{
				m_CurrentX -= m_LineSize + Param.LineSpace;
				m_CurrentY = Margin.Top + m_IndentPos;
			}
			foreach (var letter in m_Letters)
			{
				if (m_LineId == letter.LineId)
				{
					letter.LineSize = m_LineSize;
				}
			}
			m_LineId++;
		}

		class LetterParam : ILetterParam
		{
			public int LineId;
			public string Character { get; set; }
			public Vector2 Pos { get; set; }
			public Vector2 Size { get; set; }
			public string FontName { get; set; }
			public float LineSize { get; set; }
			public int FontSize => Param.FontSize;
			public LineAlign LineAlign => Param.LineAlign;
			public Color Color => Param.Color;
			public RubyLetterParam Ruby;
			public LetterGeneratorParam Param;

			public LetterParam(int lineId, string character, Vector2 pos, Vector2 size, string fontName, LetterGeneratorParam param)
			{
				LineId = lineId;
				Character = character;
				Pos = pos;
				Size = size;
				FontName = fontName;
				Param = param.Clone();
			}

			public float GetPos(bool vertical)
			{
				return vertical ? Pos.y : Pos.x;
			}

			public float GetSize(bool vertical)
			{
				return vertical ? Size.y : Size.x;
			}

			public void AddRuby(string character, float relativePos, Vector2 size, string fontName, int fontSize)
			{
				var ruby = new RubyLetterParam(this, character, relativePos, size, fontName, fontSize);
				if (Ruby != null)
				{
					Ruby.Next = ruby;
				}
				else
				{
					Ruby = ruby;
				}
			}

			public IEnumerable<IRubyLetterParam> GetRubys()
			{
				var cur = Ruby;
				while (cur != null)
				{
					yield return cur;
					cur = cur.Next;
				}
			}

		}


		class RubyLetterParam : IRubyLetterParam
		{
			ILetterParam m_Parent;
			public string Character { get; set; }
			public float RelativePos { get; set; }
			public Vector2 Size { get; set; }
			public string FontName { get; set; }
			public int FontSize { get; set; }
			public RubyLetterParam Next;

			public RubyLetterParam(ILetterParam parent, string character, float relativePos, Vector2 size, string fontName, int fontSize)
			{
				m_Parent = parent;
				Character = character;
				RelativePos = relativePos;
				Size = size;
				FontName = fontName;
				FontSize = fontSize;
			}
		}

	}


}