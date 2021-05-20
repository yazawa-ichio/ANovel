using System.Collections.Generic;

namespace ANovel.Core
{
	[TagName("macro")]
	public class MacroScope : PreProcess
	{
		[Argument(Required = true)]
		string m_Name = default;

		List<LineData> m_List = new List<LineData>();

		public void Add(in LineData data)
		{
			m_List.Add(data);
		}

		public override void Result(PreProcessor.Result result)
		{
			result.MacroDefine.Add(m_Name, m_List.ToArray());
		}
	}

	[TagName("endmacro")]
	public class EndMacroScope : PreProcess
	{
	}
}