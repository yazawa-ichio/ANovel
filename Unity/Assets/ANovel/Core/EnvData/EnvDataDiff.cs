using ANovel.Serialization;
using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public class EnvDataDiff : ICustomMapSerialization
	{
		Dictionary<Type, object> m_Dic = new Dictionary<Type, object>();

		public int Length => m_Dic.Count;

		public IEnumerable<Type> GetEntryTypes()
		{
			return m_Dic.Keys;
		}

		public void Set<TValue>(DiffData<TValue> diff)
		{
			m_Dic[typeof(TValue)] = diff;
		}

		public DiffData<TValue> Get<TValue>()
		{
			if (m_Dic.TryGetValue(typeof(TValue), out var obj))
			{
				return (DiffData<TValue>)obj;
			}
			return DiffData<TValue>.Empty;
		}

		public void Write(Writer writer)
		{
			foreach (var item in m_Dic)
			{
				writer.Write(TypeUtil.GetTypeName(item.Key));
				Packer.Pack(writer, item.Value);
			}
		}

		public void Read(int length, Reader reader)
		{
			for (int i = 0; i < length; i++)
			{
				var type = TypeUtil.GetType(reader.ReadString());
				if (!typeof(IEnvValue).IsAssignableFrom(type))
				{
					throw new InvalidOperationException($"not IEnvValue {type}");
				}
				var unpackType = typeof(DiffData<>).MakeGenericType(type);
				m_Dic[type] = Packer.Unpack(reader, unpackType);
			}
		}

	}

}