using ANovel.Core;
using System.Linq;

namespace ANovel.Commands
{
	public interface IVariableCommand
	{
		void UpdatVariables(IEvaluator evaluator);
	}

	[TagName("val")]
	public class VariableSetCommand : SystemCommand, IVariableCommand
	{
		[Argument(Required = true)]
		string m_Name;
		[Argument(Required = true)]
		string m_Value;
		[Argument]
		VariableType? m_Type;
		[Argument]
		bool m_Global = false;

		public void UpdatVariables(IEvaluator evaluator)
		{
			var variables = m_Global ? evaluator.GlobalVariables : evaluator.Variables;
			if (m_Type.HasValue)
			{
				switch (m_Type.Value)
				{
					case VariableType.Bool:
						variables.Set(m_Name, bool.Parse(m_Value));
						break;
					case VariableType.Long:
						variables.Set(m_Name, bool.Parse(m_Value));
						break;
					case VariableType.Double:
						variables.Set(m_Name, bool.Parse(m_Value));
						break;
					case VariableType.String:
						variables.Set(m_Name, m_Value);
						break;
				}
			}
			else
			{
				if (bool.TryParse(m_Value, out var boolValue))
				{
					variables.Set(m_Name, boolValue);
					return;
				}
				if (long.TryParse(m_Value, out var longValue))
				{
					variables.Set(m_Name, longValue);
					return;
				}
				if (double.TryParse(m_Value, out var doubleValue))
				{
					variables.Set(m_Name, doubleValue);
					return;
				}
				variables.Set(m_Name, m_Value);
			}
		}

	}

	[TagName("val_del")]
	public class VariableDeleteCommand : SystemCommand, IVariableCommand
	{
		[Argument(Required = true)]
		string m_Name;
		[Argument]
		bool m_Global = false;

		public void UpdatVariables(IEvaluator evaluator)
		{
			var variables = m_Global ? evaluator.GlobalVariables : evaluator.Variables;
			variables.Delete(m_Name);
		}
	}

	[TagName("val_del_all")]
	public class DeleteAllVariableCommand : SystemCommand, IVariableCommand
	{
		[Argument]
		string m_Prefix;
		[Argument]
		string m_Suffix;
		[Argument]
		string m_Contains;
		[Argument]
		bool m_Global = false;

		public void UpdatVariables(IEvaluator evaluator)
		{
			var variables = m_Global ? evaluator.GlobalVariables : evaluator.Variables;
			bool delete = false;
			if (!string.IsNullOrEmpty(m_Prefix))
			{
				delete = true;
				foreach (var key in variables.Keys.Where(x => x.StartsWith(m_Prefix)).ToArray())
				{
					variables.Delete(key);
				}
			}
			if (!string.IsNullOrEmpty(m_Suffix))
			{
				delete = true;
				foreach (var key in variables.Keys.Where(x => x.EndsWith(m_Suffix)).ToArray())
				{
					variables.Delete(key);
				}
			}
			if (!string.IsNullOrEmpty(m_Contains))
			{
				delete = true;
				foreach (var key in variables.Keys.Where(x => x.Contains(m_Contains)).ToArray())
				{
					variables.Delete(key);
				}
			}
			if (!delete)
			{
				variables.Clear();
			}
		}
	}


}