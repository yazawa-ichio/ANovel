using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{
	public class TagProvider
	{
		TagParam m_Param = new TagParam();

		public List<string> Symbols { get; private set; }

		public List<MacroDefine> Macros { get; private set; } = new List<MacroDefine>();

		public List<IParamConverter> Converters { get; private set; } = new List<IParamConverter>();

		public TagProvider() : this(new List<string>()) { }

		public TagProvider(List<string> symbols)
		{
			Symbols = symbols;
		}

		public void Setup(PreProcessResult result)
		{
			Symbols.Clear();
			Converters.Clear();
			Macros.Clear();
			Symbols.AddRange(result.Symbols);
			Converters.AddRange(result.Converters.OrderByDescending(x => x.Priority));
			Macros.Add(result.MacroDefine);
		}

		public void Provide(in LineData data, List<Tag> ret)
		{

			m_Param.Set(in data, Converters);

			foreach (var macro in Macros)
			{
				if (macro.TryProvide(Symbols, m_Param, ret))
				{
					return;
				}
			}

			if (TagEntry.TryGet(in data, m_Param.Name, Symbols, out var entry))
			{
				ret.Add(entry.Create(in data, m_Param));
			}

		}

	}

}