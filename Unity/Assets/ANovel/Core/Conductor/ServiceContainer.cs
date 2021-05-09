using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public interface IServiceContainer
	{
		T Get<T>();
		bool TryGet<T>(out T ret);
	}

	public class ServiceContainer : IServiceContainer
	{
		IServiceContainer m_Parent;
		Dictionary<Type, object> m_Dic = new Dictionary<Type, object>();

		public ServiceContainer() { }

		public ServiceContainer(IServiceContainer parent)
		{
			m_Parent = parent;
		}

		public void Set<T>(T item)
		{
			m_Dic[typeof(T)] = item;
		}

		public void Set(Type type, object item)
		{
			m_Dic[type] = item;
		}

		public T Get<T>()
		{
			if (TryGet<T>(out var ret))
			{
				return ret;
			}
			throw new KeyNotFoundException($"not found Container item {typeof(T)}");
		}

		public bool TryGet<T>(out T ret)
		{
			if (m_Dic.TryGetValue(typeof(T), out object value))
			{
				ret = (T)value;
				return true;
			}
			if (m_Parent != null)
			{
				return m_Parent.TryGet<T>(out ret);
			}
			ret = default;
			return false;
		}

		public ServiceContainer CreateChild()
		{
			return new ServiceContainer(this);
		}

		internal void Dispose()
		{
			m_Parent = null;
			m_Dic.Clear();
		}
	}
}