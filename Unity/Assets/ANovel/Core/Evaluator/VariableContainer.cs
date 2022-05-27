using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{
	public enum VariableType
	{
		Bool,
		Long,
		Double,
		String,
	}

	public interface IVariable : IEnvValue
	{
	}

	internal struct VariableBool : IVariable
	{
		public bool Value;
	}

	internal struct VariableLong : IVariable
	{
		public long Value;
	}

	internal struct VariableDouble : IVariable
	{
		public double Value;
	}

	internal struct VariableString : IVariable
	{
		public string Value;
	}

	public class VariableContainer : IVariableContainer
	{
		IEnvData m_Data;

		public IEnumerable<string> Keys => m_Data.GetAllByInterface<IVariable>().Select(x => x.Key);

		public void Init(IEnvData data)
		{
			m_Data = data.Prefixed("Variables");
		}

		public void Set(string name, long value)
		{
			m_Data.DeleteAllByInterface<IVariable>((key, v) =>
			{
				return key == name;
			});
			m_Data.Set(name, new VariableLong { Value = value });
		}

		public void Set(string name, double value)
		{
			m_Data.DeleteAllByInterface<IVariable>((key, v) =>
			{
				return key == name;
			});
			m_Data.Set(name, new VariableDouble { Value = value });
		}

		public void Set(string name, bool value)
		{
			m_Data.DeleteAllByInterface<IVariable>((key, v) =>
			{
				return key == name;
			});
			m_Data.Set(name, new VariableBool { Value = value });
		}

		public void Set(string name, string value)
		{
			m_Data.DeleteAllByInterface<IVariable>((key, v) =>
			{
				return key == name;
			});
			m_Data.Set(name, new VariableString { Value = value });
		}

		public object Get(string name)
		{
			if (TryGetValue(name, out var value))
			{
				return value;
			}
			throw new KeyNotFoundException($"variable not found {name}");
		}

		public void Clear()
		{
			m_Data.DeleteAllByInterface<IVariable>();
		}

		public bool Has(string key)
		{
			return TryGetValue(key, out var _);
		}

		public void Delete(string key)
		{
			m_Data.DeleteAllByInterface<IVariable>((name, v) =>
			{
				return key == name;
			});
		}

		public bool TryGetValue(string key, out object value)
		{
			if (m_Data.TryGet<VariableBool>(key, out var variableBool))
			{
				value = variableBool.Value.ToString().ToLower();
				return true;
			}
			if (m_Data.TryGet<VariableLong>(key, out var variableLong))
			{
				value = variableLong.Value;
				return true;
			}
			if (m_Data.TryGet<VariableDouble>(key, out var variableDouble))
			{
				value = variableDouble.Value;
				return true;
			}
			if (m_Data.TryGet<VariableString>(key, out var variableString))
			{
				value = variableString.Value;
				return true;
			}
			value = null;
			return false;
		}

	}

}