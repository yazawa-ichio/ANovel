﻿using ANovel.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{
	public class EnvDataSnapshot : ICustomMapSerialization
	{
		int ICustomMapSerialization.Length => m_Dic.Count;

		Dictionary<Type, object> m_Dic = new Dictionary<Type, object>();

		public IEnumerable<Type> GetEntryTypes()
		{
			return m_Dic.Keys;
		}

		public void Set<TValue>(Dictionary<string, TValue> data) where TValue : struct, IEnvValue
		{
			m_Dic[typeof(TValue)] = data;
		}

		public Dictionary<string, TValue> Get<TValue>() where TValue : struct, IEnvValue
		{
			if (m_Dic.TryGetValue(typeof(TValue), out var obj))
			{
				return (Dictionary<string, TValue>)obj;
			}
			return null;
		}

		public void Write(Writer writer)
		{
			foreach (var key in m_Dic.Keys.OrderBy(x => x.FullName))
			{
				writer.Write(TypeUtil.GetTypeName(key));
				Packer.Pack(writer, m_Dic[key]);
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
				var unpackType = typeof(Dictionary<,>).MakeGenericType(typeof(string), type);
				m_Dic[type] = Packer.Unpack(reader, unpackType);
			}
		}

		public static EnvDataSnapshot Marge(EnvDataSnapshot @base, EnvDataSnapshot snapshot)
		{
			var data = new EnvData();
			data.Load(@base);
			var append = new EnvData();
			append.Load(snapshot);
			data.Merge(append);
			return data.Save();
		}
	}
}