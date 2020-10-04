using System.Collections.Generic;

namespace ANovel.Core
{
	[TagName("macro", LineType.PreProcess)]
	public class MacroScope : PreProcess
	{
		[TagField(Required = true)]
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

	[TagName("endmacro", LineType.PreProcess)]
	public class EndMacroScope : PreProcess
	{
	}
}