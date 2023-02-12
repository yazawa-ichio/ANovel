using System.Collections.Generic;

namespace ANovel.Core
{
	[TagName("macro")]
	public class MacroScope : PreProcess, IPreProcessScope
	{
		[Argument(Required = true)]
		string m_Name = default;

		List<LineData> m_List = new List<LineData>();

		public void Add(in LineData data)
		{
			m_List.Add(data);
		}

		public bool IsEnd(PreProcess tag)
		{
			return tag is EndMacroScope;
		}

		public override void Result(PreProcessResult result)
		{
			result.MacroDefine.Add(m_Name, m_List.ToArray());
		}
	}

	[TagName("endmacro")]
	public class EndMacroScope : PreProcess
	{
	}
}