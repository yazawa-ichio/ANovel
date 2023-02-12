using ANovel.Core;
using System.Collections.Generic;

namespace ANovel.Actions
{

	[TagName("action")]
	public class ActionDefine : PreProcess, IPreProcessScope
	{
		[Argument]
		string m_Name;

		List<LineData> m_List = new List<LineData>();

		public void Add(in LineData data)
		{
			m_List.Add(data);
		}

		public bool IsEnd(PreProcess tag)
		{
			return tag is EndActionDefine;
		}

		public override void Result(PreProcessResult result)
		{
			var macro = new Macro(result.MacroDefine, m_List.ToArray());
			result.Meta.Set(m_Name, new ActionMetaData(m_Name, macro, result.Symbols));
		}
	}

	[TagName("endaction")]
	public class EndActionDefine : PreProcess
	{

	}

}