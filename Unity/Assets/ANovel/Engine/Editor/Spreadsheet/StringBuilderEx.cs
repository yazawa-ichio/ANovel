using System;
using System.Text;


namespace ANovel.GoogleApis
{
	internal static class StringBuilderEx
	{
		class Scope : IDisposable
		{
			StringBuilder m_Self;
			string m_End;
			public Scope(StringBuilder self, string end)
			{
				m_Self = self;
				m_End = end;
			}
			public void Dispose()
			{
				m_Self.Append(m_End);
			}
		}

		public static IDisposable AppendScope(this StringBuilder self, string begin, string end)
		{
			self.Append(begin);
			return new Scope(self, end);
		}
	}
}