using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{
	internal partial class TagEntry
	{

		Type m_Type;
		TagNameAttribute m_Attr;
		TagFieldEntry[] m_Fields;
		InjectParamEntry[] m_InjectParams;

		public string Name => m_Attr.Name;

		public LineType Type => m_Attr.Type;

		public TagEntry(Type type, TagNameAttribute attr)
		{
			m_Type = type;
			m_Attr = attr;
			m_Fields = GetFields().ToArray();
			m_InjectParams = GetInjectParams().ToArray();
		}

		IEnumerable<TagFieldEntry> GetFields()
		{
			var type = m_Type;
			while (type != typeof(Tag))
			{
				foreach (var entry in GetFields(type))
				{
					yield return entry;
				}
				type = type.BaseType;
			}
		}

		IEnumerable<InjectParamEntry> GetInjectParams()
		{
			var type = m_Type;
			while (type != typeof(Tag))
			{
				foreach (var entry in GetInjectParams(type))
				{
					yield return entry;
				}
				type = type.BaseType;
			}
		}

		public bool IsDefineSymbol(List<string> symbols)
		{
			if (string.IsNullOrEmpty(m_Attr.Symbol))
			{
				return true;
			}
			return symbols.Contains(m_Attr.Symbol);
		}

		public Tag Create(in LineData data, Dictionary<string, string> param)
		{
			var tag = (Tag)Activator.CreateInstance(m_Type);
			tag.Set(m_Attr.Name, data);
			foreach (var field in m_Fields)
			{
				if (param.TryGetValue(field.Name, out var value))
				{
					try
					{
						field.Set(tag, value);
					}
					catch (Exception ex)
					{
						throw new LineDataException(in data, $"{Name} {field.Name} format error : {value}", ex);
					}
				}
				else if (field.Required)
				{
					throw new LineDataException(in data, $"{Name} {field.Name} required key");
				}
			}
			foreach (var injectParam in m_InjectParams)
			{
				injectParam.Set(tag, param);
			}
			return tag;
		}

	}


}