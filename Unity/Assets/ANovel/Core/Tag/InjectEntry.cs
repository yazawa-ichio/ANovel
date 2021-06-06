using ANovel.Core.Define;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ANovel.Core
{
	public class InjectEntry
	{
		FieldInfo m_FieldInfo;
		PropertyInfo m_PropertyInfo;
		InjectArgumentAttribute m_Attr;
		System.Type m_FieldType;
		InjectTargetEntry m_TargetEntry;
		HashSet<string> m_Targets;
		HashSet<string> m_Ignores;

		public InjectEntry(FieldInfo info, InjectArgumentAttribute attr)
		{
			m_FieldInfo = info;
			m_FieldType = info.FieldType;
			SetAttr(attr);
		}

		public InjectEntry(PropertyInfo info, InjectArgumentAttribute attr)
		{
			m_PropertyInfo = info;
			m_FieldType = info.PropertyType;
			SetAttr(attr);
		}

		void SetAttr(InjectArgumentAttribute attr)
		{
			m_Attr = attr;
			m_TargetEntry = InjectTargetEntry.Get(m_FieldType);
			m_Attr.TryGetTargetKeys(out m_Targets);
			m_Attr.TryGetIgnoreKeys(out m_Ignores);
		}

		public void Set(Tag tag, Dictionary<string, string> param)
		{
			var target = GetTarget(tag) ?? SetTarget(tag);
			m_TargetEntry.Set(tag, target, param, m_Targets, m_Ignores);
			if (m_FieldType.IsValueType)
			{
				SetTarget(tag, target);
			}
		}

		object GetTarget(Tag tag)
		{
			if (m_FieldInfo != null)
			{
				return m_FieldInfo.GetValue(tag);
			}
			return m_PropertyInfo.GetValue(tag);
		}

		object SetTarget(Tag tag)
		{
			var target = Activator.CreateInstance(m_FieldType);
			if (m_FieldInfo != null)
			{
				m_FieldInfo.SetValue(tag, target);
			}
			else
			{
				m_PropertyInfo.SetValue(tag, target);
			}
			return target;
		}

		void SetTarget(Tag tag, object target)
		{
			if (m_FieldInfo != null)
			{
				m_FieldInfo.SetValue(tag, target);
			}
			else
			{
				m_PropertyInfo.SetValue(tag, target);
			}
		}

		public IEnumerable<ArgumentDefine> CreateDefine()
		{
			Attribute[] attributes;
			if (m_FieldInfo != null)
			{
				attributes = m_FieldInfo.GetCustomAttributes().ToArray();
			}
			else
			{
				attributes = m_PropertyInfo.GetCustomAttributes().ToArray();
			}
			return m_TargetEntry.CreateDefine(m_Targets, m_Ignores, attributes);
		}
	}

}