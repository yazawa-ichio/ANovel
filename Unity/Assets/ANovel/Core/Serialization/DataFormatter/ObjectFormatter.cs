using System;
using System.Collections;
using System.Collections.Generic;

namespace ANovel.Serialization
{
	public class ObjectFormatter : DataFormatter
	{
		class DicTypeInfo
		{
			public Type KeyType;
			public Type ValueType;
		}

		static Dictionary<Type, DicTypeInfo> s_DicTypeInfo = new Dictionary<Type, DicTypeInfo>();
		static DicTypeInfo GetDicTypeInfo(Type type)
		{
			if (!s_DicTypeInfo.TryGetValue(type, out var value))
			{
				s_DicTypeInfo[type] = value = new DicTypeInfo();
				var args = type.GetGenericArguments();
				value.KeyType = args[0];
				value.ValueType = args[1];
			}
			return value;
		}


		protected override void Write(Writer writer, object obj)
		{
			if (obj is ICustomMapSerialization customMap)
			{
				writer.WriteMapHeader(customMap.Length);
				customMap.Write(writer);
			}
			else if (obj is ICustomArraySerialization customArray)
			{
				writer.WriteArrayHeader(customArray.Length);
				customArray.Write(writer);
			}
			else if (obj is IDictionary dic)
			{
				writer.WriteMapHeader(dic.Count);
				foreach (var key in dic.Keys)
				{
					Packer.Pack(writer, key);
					Packer.Pack(writer, dic[key]);
				}
			}
			else if (obj is IList list)
			{
				writer.WriteArrayHeader(list.Count);
				for (int i = 0; i < list.Count; i++)
				{
					Packer.Pack(writer, list[i]);
				}
			}
			else
			{
				var fields = ReflectionCache.Get(obj.GetType()).Fields;
				writer.WriteMapHeader(fields.Count);
				foreach (var kvp in fields)
				{
					writer.WriteRefString(kvp.Key);
					Packer.Pack(writer, kvp.Value.Get(obj));
				}
			}
		}

		protected override object Read(Reader reader, Type type)
		{
			var code = reader.PeekCode();
			if (code == DataTypeCode.Array)
			{
				return ReadArray(reader, type);
			}
			else
			{
				return ReadMap(reader, type);
			}
		}

		object ReadMap(Reader reader, Type type)
		{
			var length = reader.ReadMap();
			var obj = Activator.CreateInstance(type);
			if (obj is ICustomMapSerialization custom)
			{
				custom.Read(length, reader);
			}
			else if (obj is IDictionary dic)
			{
				var info = GetDicTypeInfo(type);
				for (int i = 0; i < length; i++)
				{
					var key = Packer.Unpack(reader, info.KeyType);
					var value = Packer.Unpack(reader, info.ValueType);
					dic[key] = value;
				}
			}
			else
			{
				var fields = ReflectionCache.Get(type).Fields;
				for (int i = 0; i < length; i++)
				{
					var key = reader.ReadString();
					if (fields.TryGetValue(key, out var field))
					{
						field.Set(obj, Packer.Unpack(reader, field.Type));
					}
					else
					{
						reader.ReadSkip();
					}
				}
			}
			return obj;
		}

		object ReadArray(Reader reader, Type type)
		{
			var length = reader.ReadArray();
			if (type.IsArray)
			{
				var elementType = type.GetElementType();
				var array = Array.CreateInstance(elementType, length);
				for (int i = 0; i < length; i++)
				{
					array.SetValue(Packer.Unpack(reader, elementType), i);
				}
				return array;
			}
			else
			{
				var obj = Activator.CreateInstance(type);
				if (obj is ICustomArraySerialization custom)
				{
					custom.Read(length, reader);
				}
				else
				{
					var list = (IList)Activator.CreateInstance(type);
					var elementType = type.GetGenericArguments()[0];
					for (int i = 0; i < length; i++)
					{
						list.Add(Packer.Unpack(reader, elementType));
					}
				}
				return obj;
			}
		}


	}


}
