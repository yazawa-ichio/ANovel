using System;
using System.Collections.Generic;
using System.Reflection;

namespace ANovel.Core
{
	public class InjectParamEntry
	{
		FieldInfo m_FieldInfo;
		PropertyInfo m_PropertyInfo;
		InjectParamAttribute m_Attr;
		System.Type m_FieldType;
		InjectParamTargetEntry m_TargetEntry;
		HashSet<string> m_Targets;

		public InjectParamEntry(FieldInfo info, InjectParamAttribute attr)
		{
			m_FieldInfo = info;
			m_FieldType = info.FieldType;
			SetAttr(attr);
		}

		public InjectParamEntry(PropertyInfo info, InjectParamAttribute attr)
		{
			m_PropertyInfo = info;
			m_FieldType = info.PropertyType;
			SetAttr(attr);
		}

		void SetAttr(InjectParamAttribute attr)
		{
			m_Attr = attr;
			m_TargetEntry = InjectParamTargetEntry.Get(m_FieldType);
			if (m_Attr.Keys != null)
			{
				m_Targets = new HashSet<string>(m_Attr.Keys);
			}
		}

		public void Set(Tag tag, Dictionary<string, string> param)
		{
			var target = GetTarget(tag) ?? SetTarget(tag);
			m_TargetEntry.Set(tag, target, param, m_Targets);
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

	}

}
