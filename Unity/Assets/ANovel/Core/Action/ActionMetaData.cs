using ANovel.Core;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Actions
{
	public class ActionMetaData
	{
		public static ActionData CreateData(string name, Command command, IEnvData data)
		{
			return command.Meta.Get<ActionMetaData>(name).CreateData(command, data);
		}

		public string Name { get; private set; }
		Macro m_Macro;
		List<string> m_Symbols;

		public ActionMetaData(string name, Macro macro, List<string> symbols)
		{
			Name = name;
			m_Macro = macro;
			m_Symbols = symbols;
		}


		public ActionData CreateData(Command command, IEnvData data)
		{
			return CreateData(command, data, command.Dic, command.Container.Get<IEvaluator>(), command.Meta);
		}

		public ActionData CreateData(object target, IEnvData data, IReadOnlyDictionary<string, string> dic, IEvaluator evaluator, IMetaData meta)
		{
			TagParam param = new TagParam();
			param.Evaluator = evaluator;
			foreach (var kvp in dic)
			{
				param.AddValue(kvp.Key, kvp.Value);
			}
			var builder = new ActionParamContext(data, meta);
			foreach (var cmd in m_Macro.Provide(m_Symbols, param).Cast<IActionParamCommand>())
			{
				cmd.AddParam(target, builder);
			}
			return builder.CreateData();
		}

	}
}