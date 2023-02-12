using ANovel.Commands;
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

		public BranchController BranchController { get; private set; } = new BranchController();

		public TagProvider() : this(null, new List<string>()) { }

		public TagProvider(IEvaluator evaluator, List<string> symbols)
		{
			Symbols = symbols;
			m_Param.Evaluator = evaluator;
		}

		public void Setup(PreProcessResult result)
		{
			Symbols.Clear();
			Converters.Clear();
			Macros.Clear();
			BranchController.Clear();

			Symbols.AddRange(result.Symbols);
			Converters.AddRange(result.Converters.OrderByDescending(x => x.Priority));
			Macros.Add(result.MacroDefine);
		}

		public void Provide(in LineData data, List<Tag> ret)
		{
			ret.AddRange(Provide(data));
		}

		public IEnumerable<Tag> Provide(LineData data)
		{

			m_Param.Set(in data, Converters);

			BranchController.TryPrepare(m_Param);

			if (BranchController.CheckIgnore(m_Param))
			{
				yield break;
			}

			foreach (var macro in Macros)
			{
				if (macro.TryProvide(Symbols, m_Param, out var ret))
				{
					foreach (var tag in ret)
					{
						yield return tag;
					}
					yield break;
				}
			}

			if (TagEntry.TryGet(in data, m_Param.Name, Symbols, out var entry))
			{
				var tag = entry.Create(in data, m_Param);
				if (tag is IVariableCommand variableCommand)
				{
					variableCommand.UpdatVariables(m_Param.Evaluator);
				}
				if (tag is IBranchCommand branchCommand)
				{
					BranchController.BranchCommand(m_Param.Evaluator, branchCommand);
					yield break;
				}
				if (tag is IBranchEnd branchEnd)
				{
					BranchController.BranchEnd(branchEnd);
					yield break;
				}
				yield return tag;
			}

		}

	}

}