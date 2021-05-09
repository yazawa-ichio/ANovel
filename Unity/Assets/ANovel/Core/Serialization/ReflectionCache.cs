using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;


namespace ANovel.Serialization
{

	public class ReflectionCache
	{

		static ConcurrentDictionary<Type, ReflectionCache> s_Dic = new ConcurrentDictionary<Type, ReflectionCache>();

		static Func<Type, ReflectionCache> s_Create;

		static ReflectionCache()
		{
			s_Create = (x) => new ReflectionCache(x);
		}

		public static ReflectionCache Get(Type type)
		{
			return s_Dic.GetOrAdd(type, s_Create);
		}

		public Type Type { get; private set; }

		public IReadOnlyDictionary<string, FieldEntry> Fields => m_Fields;

		Dictionary<string, FieldEntry> m_Fields;

		public ReflectionCache(Type type)
		{
			Type = type;
			m_Fields = new Dictionary<string, FieldEntry>();
			foreach (var f in type.GetFields())
			{
				if (!f.IsStatic)
				{
					m_Fields[f.Name] = new FieldEntry(f);
				}
			}
			foreach (var p in type.GetProperties())
			{
				if (p.CanWrite && p.CanRead && !p.SetMethod.IsStatic && !p.GetMethod.IsStatic)
				{
					m_Fields[p.Name] = new FieldEntry(p);
				}
			}
		}

		public object CreateInstance()
		{
			return Activator.CreateInstance(Type);
		}

		public class FieldEntry
		{
			FieldInfo m_Field;
			PropertyInfo m_Property;

			public Type Type { get; private set; }

			public FieldEntry(FieldInfo field)
			{
				m_Field = field;
				Type = field.FieldType;
			}

			public FieldEntry(PropertyInfo property)
			{
				m_Property = property;
				Type = property.PropertyType;
			}

			public object Get(object obj)
			{
				return m_Field?.GetValue(obj) ?? m_Property?.GetValue(obj);
			}

			public void Set(object obj, object value)
			{
				m_Field?.SetValue(obj, value);
				m_Property?.SetValue(obj, value);
			}
		}


	}
}