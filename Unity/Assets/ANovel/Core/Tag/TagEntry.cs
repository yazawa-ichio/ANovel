using ANovel.Core.Define;
using System;
using System.Collections.Generic;
using System.Linq;

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


		public Tag Create(in LineData data, TagParam param)
		{
			return Create(in data, param.ToDictionary());
		}

		public Tag Create(in LineData data, IReadOnlyDictionary<string, string> dic)
		{
			Prepare();
			var tag = (Tag)Activator.CreateInstance(m_Type);
			tag.Set(m_Attr.Name, data, dic);
			foreach (var field in m_Fields)
			{
				if (dic.TryGetValue(field.Name, out var value))
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
				inject.Set(tag, dic);
			}
			return tag;
		}

		public TagDefine CreateDefine()
		{
			Prepare();
			TagDefine ret = new TagDefine();
			ret.Name = Name;
			ret.Symbols = m_Attr?.Symbol;
			ret.LineType = Token.Get(Type).ToString();
			ret.Description = DescriptionAttribute.Get(m_Type);
			ret.Arguments = GetArgumentDefines().ToArray();
			return ret;
		}

		IEnumerable<ArgumentDefine> GetArgumentDefines()
		{
			foreach (var f in m_Fields)
			{
				yield return f.CreateDefine(Array.Empty<Attribute>());
			}
			foreach (var e in m_Injects)
			{
				foreach (var d in e.CreateDefine())
				{
					yield return d;
				}
			}
		}

	}

}