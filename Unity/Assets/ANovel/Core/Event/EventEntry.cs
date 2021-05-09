using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ANovel.Core
{
	internal class EventEntry
	{
		static Dictionary<Type, EventEntry> s_Dic = new Dictionary<Type, EventEntry>();

		public static EventEntry Get(Type type)
		{
			if (!s_Dic.TryGetValue(type, out var holder))
			{
				s_Dic[type] = holder = new EventEntry(type, GetMethods(type));
			}
			return holder;
		}

		static EventMethod[] GetMethods(Type type)
		{
			using (ListPool<EventMethod>.Use(out var list))
			{
				while (type != null)
				{
					foreach (var method in EventMethod.Get(type))
					{
						list.Add(method);
					}
					type = type.BaseType;
				}
				return list.ToArray();
			}
		}

		Type m_Type;
		EventMethod[] m_Methods;
		HashSet<string> m_HashSet;

		private EventEntry(Type type, EventMethod[] methods)
		{
			m_Type = type;
			m_Methods = methods;
		}

		HashSet<string> GetHashSet()
		{
			if (m_HashSet == null)
			{
				m_HashSet = new HashSet<string>(m_Methods.Select(x => x.Name));
			}
			return m_HashSet;
		}

		public void Invoke(object obj, string name)
		{
			if (!GetHashSet().Contains(name))
			{
				return;
			}
			foreach (var method in m_Methods)
			{
				if (method.Name == name)
				{
					method.Invoke(obj, null);
				}
			}
		}

		public void Invoke(object obj, string name, params object[] prm)
		{
			if (!GetHashSet().Contains(name))
			{
				return;
			}
			foreach (var method in m_Methods)
			{
				if (method.Name == name)
				{
					method.Invoke(obj, prm);
				}
			}
		}

	}

	internal class EventMethod
	{
		static Dictionary<Type, EventMethod[]> s_Dic = new Dictionary<Type, EventMethod[]>();

		public static EventMethod[] Get(Type type)
		{
			if (!s_Dic.TryGetValue(type, out var methods))
			{
				s_Dic[type] = methods = GetAll(type);
			}
			return methods;
		}

		static EventMethod[] GetAll(Type type)
		{
			const BindingFlags Flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			using (ListPool<EventMethod>.Use(out var list))
			{
				foreach (var method in type.GetMethods(Flags))
				{
					if (!method.IsDefined(typeof(EventSubscribeAttribute), inherit: false))
					{
						continue;
					}
					foreach (var attr in method.GetCustomAttributes(typeof(EventSubscribeAttribute), inherit: false))
					{
						var entry = new EventMethod(method, attr as EventSubscribeAttribute);
						list.Add(entry);
					}
				}
				return list.ToArray();
			}
		}

		MethodInfo m_Method;
		EventSubscribeAttribute m_Attr;

		public readonly bool IsStatic;

		public readonly string Name;

		public EventMethod(MethodInfo method, EventSubscribeAttribute attr)
		{
			IsStatic = method.IsStatic;
			m_Method = method;
			m_Attr = attr;
			Name = attr.Name;
		}

		public void Invoke(object obj)
		{
			m_Method.Invoke(obj, null);
		}

		public void Invoke(object obj, object[] prm)
		{
			m_Method.Invoke(obj, prm);
		}

	}


}