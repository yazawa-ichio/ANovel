using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{
	public enum VariableType
	{
		Bool,
		Int,
		Real,
		String,
	}

	public interface IVariable : IEnvValue
	{
	}

	internal struct VariableBool : IVariable
	{
		public bool Value;
	}

	internal struct VariableInteger : IVariable
	{
		public long Value;
	}

	internal struct VariableReal : IVariable
	{
		public double Value;
	}

	internal struct VariableString : IVariable
	{
		public string Value;
	}

	public class VariableContainer : IVariableContainer
	{
		IEnvData m_Data = new EnvData();

		public IEnumerable<string> Keys => m_Data.GetAllByInterface<IVariable>().Select(x => x.Key);

		public void SetEnvData(IEnvData data)
		{
			m_Data = data.Prefixed("Variables");
		}

		public void Set(string name, long value)
		{
			m_Data.DeleteAllByInterface<IVariable>((key, v) =>
			{
				return key == name;
			});
			m_Data.Set(name, new VariableInteger { Value = value });
		}

		public void Set(string name, double value)
		{
			m_Data.DeleteAllByInterface<IVariable>((key, v) =>
			{
				return key == name;
			});
			m_Data.Set(name, new VariableReal { Value = value });
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
				value = variableBool.Value;
				return true;
			}
			if (m_Data.TryGet<VariableInteger>(key, out var variableLong))
			{
				value = variableLong.Value;
				return true;
			}
			if (m_Data.TryGet<VariableReal>(key, out var variableDouble))
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

		public byte[] Save()
		{
			var data = new EnvData();
			foreach (var kvp in m_Data.GetAll<VariableBool>())
			{
				data.Set(kvp.Key, kvp.Value);
			}
			foreach (var kvp in m_Data.GetAll<VariableInteger>())
			{
				data.Set(kvp.Key, kvp.Value);
			}
			foreach (var kvp in m_Data.GetAll<VariableReal>())
			{
				data.Set(kvp.Key, kvp.Value);
			}
			foreach (var kvp in m_Data.GetAll<VariableString>())
			{
				data.Set(kvp.Key, kvp.Value);
			}
			return Packer.Pack(data.Save());
		}

		public void Load(byte[] data)
		{
			var src = new EnvData();
			src.Load(Packer.Unpack<EnvDataSnapshot>(data));
			Clear();
			foreach (var kvp in src.GetAll<VariableBool>())
			{
				m_Data.Set(kvp.Key, kvp.Value);
			}
			foreach (var kvp in src.GetAll<VariableInteger>())
			{
				m_Data.Set(kvp.Key, kvp.Value);
			}
			foreach (var kvp in src.GetAll<VariableReal>())
			{
				m_Data.Set(kvp.Key, kvp.Value);
			}
			foreach (var kvp in src.GetAll<VariableString>())
			{
				m_Data.Set(kvp.Key, kvp.Value);
			}
		}
	}

}