

using ANovel.Core.Define;
using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public class InjectTargetEntry
	{
		static Dictionary<Type, InjectTargetEntry> s_Dic = new Dictionary<Type, InjectTargetEntry>();

		public static InjectTargetEntry Get(Type type)
		{
			if (!s_Dic.TryGetValue(type, out var value))
			{
				s_Dic[type] = value = new InjectTargetEntry(type);
			}
			return value;
		}

		Type m_Type;
		Dictionary<string, TagFieldEntry[]> m_Fields;
		string[] m_Required;

		public InjectTargetEntry(Type type)
		{
			m_Type = type;
		}

		static Dictionary<string, List<TagFieldEntry>> s_FieldsTemp = new Dictionary<string, List<TagFieldEntry>>();
		void TrySetFields()
		{
			if (m_Fields == null)
			{
				try
				{
					var fields = GetFields();
					using (ListPool<string>.Use(out var required))
					{
						foreach (var f in fields)
						{
							if (!s_FieldsTemp.TryGetValue(f.Name, out var list))
							{
								s_FieldsTemp[f.Name] = list = ListPool<TagFieldEntry>.Pop();
							}
							list.Add(f);
							if (f.Required)
							{
								required.Add(f.Name);
							}
						}
						m_Required = required.ToArray();
					}
					m_Fields = new Dictionary<string, TagFieldEntry[]>(s_FieldsTemp.Count);
					foreach (var kvp in s_FieldsTemp)
					{
						m_Fields[kvp.Key] = kvp.Value.ToArray();
					}
				}
				finally
				{
					foreach (var list in s_FieldsTemp.Values)
					{
						ListPool<TagFieldEntry>.Push(list);
					}
					s_FieldsTemp.Clear();
				}
			}
		}

		TagFieldEntry[] GetFields()
		{
			using (ListPool<TagFieldEntry>.Use(out var list))
			{
				var type = m_Type;
				while (type != null)
				{
					foreach (var entry in TagEntry.GetFields(type))
					{
						list.Add(entry);
					}
					type = type.BaseType;
				}
				foreach (var field in m_Type.GetFields())
				{
					if (field.IsStatic)
					{
						continue;
					}
					if (!field.IsDefined(typeof(ArgumentAttribute), inherit: false) && !field.IsDefined(typeof(NonSerializedAttribute), inherit: false) && !field.IsDefined(typeof(SkipArgumentAttribute), inherit: false))
					{
						list.Add(new TagFieldEntry(field, null));
					}
				}
				foreach (var property in m_Type.GetProperties())
				{
					if (property.SetMethod == null || property.SetMethod.IsStatic)
					{
						continue;
					}
					if (!property.IsDefined(typeof(ArgumentAttribute), inherit: false) && !property.IsDefined(typeof(SkipArgumentAttribute), inherit: false))
					{
						list.Add(new TagFieldEntry(property, null));
					}
				}
				return list.ToArray();
			}
		}

		public void Set(Tag tag, object target, Dictionary<string, string> param, HashSet<string> targets, HashSet<string> ignores)
		{
			TrySetFields();
			foreach (var required in m_Required)
			{
				if (ignores != null && ignores.Contains(required))
				{
					continue;
				}
				if (!param.ContainsKey(required))
				{
					throw new LineDataException(tag.LineData, $"{tag.TagName} {required} required key");
				}
			}
			foreach (var kvp in param)
			{
				if (ignores != null && ignores.Contains(kvp.Key))
				{
					continue;
				}
				if (targets != null && !targets.Contains(kvp.Key))
				{
					continue;
				}
				if (m_Fields.TryGetValue(kvp.Key, out var list))
				{
					foreach (var entry in list)
					{
						try
						{
							entry.Set(target, kvp.Value);
						}
						catch (Exception ex)
						{
							throw new LineDataException(tag.LineData, $"{tag.TagName} {entry.Name} format error : {kvp.Value}", ex);
						}
					}
				}
			}
		}

		public IEnumerable<ArgumentDefine> CreateDefine(HashSet<string> targets, HashSet<string> ignores, Attribute[] attributes)
		{
			TrySetFields();
			foreach (var kvp in m_Fields)
			{
				if (ignores != null && ignores.Contains(kvp.Key))
				{
					continue;
				}
				if (targets != null && !targets.Contains(kvp.Key))
				{
					continue;
				}
				foreach (var f in kvp.Value)
				{
					yield return f.CreateDefine(attributes);
				}
			}
		}

	}

}