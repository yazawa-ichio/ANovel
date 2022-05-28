using Jace;
using System;
using System.Text;

namespace ANovel.Core
{
	public class Evaluator : IEvaluator
	{
		StringBuilder m_Body = new StringBuilder();
		StringBuilder m_Key = new StringBuilder();
		StringBuilder m_Format = new StringBuilder();
		VariableContainer m_Container = new VariableContainer();
		VariableContainer m_GlobalContainer = new VariableContainer();
		CalculationEngine m_CalculationEngine = new CalculationEngine();

		public IVariableContainer Variables => m_Container;

		public IVariableContainer GlobalVariables => m_Container;

		public void Init(EnvData data)
		{
			m_Container.Init(data.Prefixed<Evaluator>());
			m_GlobalContainer.Init(new EnvData());
		}

		public object Eval(string value)
		{
			return Eval(value, null);
		}

		public object Eval(string value, LineData? data)
		{
			// {{ と }} はエスケープする
			var ret = m_Body.Clear();
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				if (c == '{')
				{
					i++;
					if (i < value.Length && value[i] == '{')
					{
						ret.Append(c);
						continue;
					}
					ret.Append(GetValue(ref i, value, data));
				}
				else if (c == '}')
				{
					i++;
					// }}は飛ばす
					if (i < value.Length && value[i] == '}')
					{
						i++;
					}
					ret.Append(c);
				}
				else
				{
					ret.Append(c);
				}
			}
			return ret.ToString();
		}

		string GetValue(ref int i, string value, LineData? data)
		{
			var hasFormatSuffix = false;
			var key = m_Key.Clear();
			var format = m_Format.Clear();
			format.Append("{");
			format.Append(0);
			while (i < value.Length)
			{
				var c = value[i];
				if (c == '{')
				{
					if (i + 1 < value.Length && value[i + 1] == '{')
					{
						i++;
						i++;
						key.Append(c);
						continue;
					}
					if (data.HasValue)
					{
						throw new LineDataException(data.Value, $"format error duplicate ({{) {value}");
					}
					else
					{
						throw new System.FormatException($"format error duplicate ({{) {value}");
					}
				}
				else if (c == '}')
				{
					i++;
					if (i < value.Length && value[i] == '}')
					{
						i++;
						key.Append(c);
						continue;
					}
					format.Append(c);
					break;
				}
				if (c == ':' || hasFormatSuffix)
				{
					hasFormatSuffix = true;
					format.Append(c);
					i++;
					continue;
				}
				key.Append(c);
				i++;
			}
			if (m_Container.TryGetValue(key.ToString(), out var val) || m_GlobalContainer.TryGetValue(key.ToString(), out val))
			{
				if (hasFormatSuffix)
				{
					val = string.Format(format.ToString(), val);
				}
				return val.ToString();
			}
			if (data.HasValue)
			{
				throw new LineDataException(data.Value, $"not found Variable {key}. {value}");
			}
			else
			{
				throw new System.FormatException($"not found Variable {key}. {value}");
			}
		}

		public bool Condition(string value, LineData? data)
		{
			{
				if (bool.TryParse(value, out var ret))
				{
					return ret;
				}
			}
			try
			{
				var ret = m_CalculationEngine.Calculate(value);
				return ret == 1.0d;
			}
			catch (Exception err)
			{
				if (data.HasValue)
				{
					throw new LineDataException(data.Value, $"calculate error {value}", err);
				}
				throw;
			}
		}
	}

}