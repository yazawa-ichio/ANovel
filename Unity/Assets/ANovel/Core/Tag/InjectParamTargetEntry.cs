

using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{
	public class InjectParamTargetEntry
	{
		static Dictionary<Type, InjectParamTargetEntry> s_Dic = new Dictionary<Type, InjectParamTargetEntry>();

		public static InjectParamTargetEntry Get(Type type)
		{
			if (!s_Dic.TryGetValue(type, out var value))
			{
				s_Dic[type] = value = new InjectParamTargetEntry(type);
			}
			return value;
		}

		Type m_Type;
		TagFieldEntry[] m_Fields;

		public InjectParamTargetEntry(Type type)
		{
			m_Type = type;
			SetFields();
		}

		void SetFields()
		{
			m_Fields = GetPublicFields().Concat(GetFields()).ToArray();
		}

		IEnumerable<TagFieldEntry> GetFields()
		{
			var type = m_Type;
			while (type != null)
			{
				foreach (var entry in TagEntry.GetFields(type))
				{
					yield return entry;
				}
				type = type.BaseType;
			}
		}


		IEnumerable<TagFieldEntry> GetPublicFields()
		{
			foreach (var field in m_Type.GetFields())
			{
				if (field.IsStatic)
				{
					continue;
				}
				if (!Attribute.IsDefined(field, typeof(TagFieldAttribute)) && !Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
				{
					yield return new TagFieldEntry(field, null);
				}
			}
			foreach (var property in m_Type.GetProperties())
			{
				if (property.SetMethod == null)
				{
					continue;
				}
				if (!Attribute.IsDefined(property, typeof(TagFieldAttribute)) && !Attribute.IsDefined(property, typeof(NonSerializedAttribute)))
				{
					yield return new TagFieldEntry(property, null);
				}
			}
		}

		public void Set(Tag tag, object target, Dictionary<string, string> param, HashSet<string> targets)
		{
			foreach (var field in m_Fields)
			{
				if (targets != null && !targets.Contains(field.Name))
				{
					continue;
				}
				if (param.TryGetValue(field.Name, out var value))
				{
					try
					{
						field.Set(target, value);
					}
					catch (Exception ex)
					{
						throw new LineDataException(tag.LineData, $"{tag.TagName} {field.Name} format error : {value}", ex);
					}
				}
				else if (field.Required)
				{
					throw new LineDataException(tag.LineData, $"{tag.TagName} {field.Name} required key");
				}
			}
		}

	}

}
