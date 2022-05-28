using ANovel.Core;
using System;
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
				try
				{
					switch (m_Type.Value)
					{
						case VariableType.Bool:
							variables.Set(m_Name, bool.Parse(m_Value));
							break;
						case VariableType.Int:
							variables.Set(m_Name, long.Parse(m_Value));
							break;
						case VariableType.Real:
							variables.Set(m_Name, double.Parse(m_Value));
							break;
						case VariableType.String:
							variables.Set(m_Name, m_Value);
							break;
					}
				}
				catch (Exception err)
				{
					throw new LineDataException(LineData, $"parse error {m_Value}", err);
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

	[TagName("val_eval")]
	public class VariableEvalCommand : SystemCommand, IVariableCommand
	{
		[Argument(Required = true)]
		string m_Name;
		[Argument(Required = true)]
		string m_Value;
		[Argument]
		VariableType? m_Type;
		[Argument]
		bool m_Global;

		public void UpdatVariables(IEvaluator evaluator)
		{
			var variables = m_Global ? evaluator.GlobalVariables : evaluator.Variables;
			var ret = evaluator.Eval(m_Value, LineData);
			if (m_Type.HasValue)
			{
				switch (m_Type.Value)
				{
					case VariableType.Bool:
						variables.Set(m_Name, ret == 1.0d);
						break;
					case VariableType.Int:
						variables.Set(m_Name, (int)ret);
						break;
					case VariableType.Real:
						variables.Set(m_Name, ret);
						break;
					case VariableType.String:
						variables.Set(m_Name, ret.ToString());
						break;
				}
			}
			else
			{
				if (m_Value.Contains(">") || m_Value.Contains("<") || m_Value.Contains("="))
				{
					variables.Set(m_Name, ret == 1.0d);
				}
				else
				{
					variables.Set(m_Name, ret);
				}
			}
		}

	}

	[TagName("val_add")]
	public class VariableAddCommand : SystemCommand, IVariableCommand
	{
		[Argument(Required = true)]
		string m_Name;
		[Argument]
		long m_Value = 1;
		[Argument(KeyName = "allow_empty")]
		bool m_AllowEmpty = true;
		[Argument]
		bool m_Global = false;

		public void UpdatVariables(IEvaluator evaluator)
		{
			var variables = m_Global ? evaluator.GlobalVariables : evaluator.Variables;
			if (!variables.TryGetValue(m_Name, out var val))
			{
				if (!m_AllowEmpty)
				{
					throw new LineDataException(LineData, $"variable not found {m_Name}");
				}
				val = (long)0;
			}
			if (val is long longVal)
			{
				longVal += m_Value;
				variables.Set(m_Name, longVal);
			}
			else
			{
				throw new LineDataException(LineData, $"variable is not long. {m_Name}:{val}");
			}
		}
	}

	[TagName("flag")]
	public class VariableFlagCommand : SystemCommand, IVariableCommand
	{
		[Argument(Required = true)]
		string m_Name;
		[Argument]
		bool? m_On;
		[Argument]
		bool? m_Off;
		[Argument]
		bool m_Global = false;

		public void UpdatVariables(IEvaluator evaluator)
		{
			if (m_On.HasValue && m_Off.HasValue)
			{
				throw new LineDataException(LineData, "not allow on and off.");
			}
			var variables = m_Global ? evaluator.GlobalVariables : evaluator.Variables;
			if (m_On.HasValue)
			{
				variables.Set(m_Name, m_On.Value);
			}
			else if (m_Off.HasValue)
			{
				variables.Set(m_Name, !m_Off.Value);
			}
			else
			{
				variables.Set(m_Name, true);
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
	public class VariableDeleteAllCommand : SystemCommand, IVariableCommand
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