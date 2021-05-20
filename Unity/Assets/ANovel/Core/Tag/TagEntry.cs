using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	internal partial class TagEntry
	{

		Type m_Type;
		TagNameAttribute m_Attr;
		bool m_Prepare;
		TagFieldEntry[] m_Fields;
		InjectEntry[] m_Injects;

		public string Name => m_Attr.Name;

		public LineType Type { get; private set; }

		public TagEntry(Type type, TagNameAttribute attr)
		{
			if (typeof(PreProcess).IsAssignableFrom(type))
			{
				Type = LineType.PreProcess;
			}
			else if (typeof(ISystemCommand).IsAssignableFrom(type))
			{
				Type = LineType.SystemCommand;
			}
			else
			{
				Type = LineType.Command;
			}
			m_Type = type;
			m_Attr = attr;
		}

		public void Prepare()
		{
			if (!m_Prepare)
			{
				m_Prepare = true;
				m_Fields = GetFields();
				m_Injects = GetInjects();
			}
		}

		TagFieldEntry[] GetFields()
		{
			using (ListPool<TagFieldEntry>.Use(out var list))
			{
				var type = m_Type;
				while (type != typeof(Tag))
				{
					foreach (var entry in GetFields(type))
					{
						list.Add(entry);
					}
					type = type.BaseType;
				}
				return list.ToArray();
			}
		}

		InjectEntry[] GetInjects()
		{
			using (ListPool<InjectEntry>.Use(out var list))
			{
				var type = m_Type;
				while (type != typeof(Tag))
				{
					foreach (var entry in GetInjects(type))
					{
						list.Add(entry);
					}
					type = type.BaseType;
				}
				return list.ToArray();
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
			Prepare();
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
			foreach (var inject in m_Injects)
			{
				inject.Set(tag, param);
			}
			return tag;
		}

	}


}