using System;
using System.Reflection;

namespace ANovel.Core
{
	internal class TagFieldEntry
	{
		FieldInfo m_FieldInfo;
		PropertyInfo m_PropertyInfo;
		IFormatter m_Formatter;
		TagFieldAttribute m_Attr;
		Type m_FieldType;
		bool m_InitFormatter;

		public bool Required => m_Attr?.Required ?? false;

		public string Name { get; private set; }

		public TagFieldEntry(FieldInfo info, TagFieldAttribute attr)
		{
			m_FieldInfo = info;
			m_FieldType = info.FieldType;
			Name = ConvertName(info.Name);
			SetAttr(attr);
		}

		public TagFieldEntry(PropertyInfo info, TagFieldAttribute attr)
		{
			m_PropertyInfo = info;
			m_FieldType = info.PropertyType;
			Name = ConvertName(info.Name);
			SetAttr(attr);
		}

		string ConvertName(string name)
		{
			if (name.StartsWith("_", StringComparison.Ordinal))
			{
				return name.Substring(1).ToLower();
			}
			else if (name.StartsWith("m_", StringComparison.Ordinal))
			{
				return name.Substring(2).ToLower();
			}
			return name.ToLower();
		}

		void SetAttr(TagFieldAttribute attr)
		{
			if (attr == null)
			{
				return;
			}
			m_Attr = attr;
			if (!string.IsNullOrEmpty(attr.KeyName))
			{
				Name = attr.KeyName.ToLower();
			}
		}

		void TryInit()
		{
			if (!m_InitFormatter)
			{
				m_InitFormatter = true;
				if (m_Attr != null && m_Attr.Formatter != null)
				{
					m_Formatter = Formatter.Get(m_Attr.Formatter);
				}
			}
		}

		public void Set(object obj, string value)
		{
			TryInit();
			object val;
			if (m_Formatter != null)
			{
				val = m_Formatter.Format(value);
			}
			else
			{
				val = Formatter.Format(m_FieldType, value);
			}
			m_FieldInfo?.SetValue(obj, val);
			m_PropertyInfo?.SetValue(obj, val);
		}

	}


}