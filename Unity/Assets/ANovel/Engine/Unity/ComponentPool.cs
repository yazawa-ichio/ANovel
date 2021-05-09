using System;
using System.Collections.Generic;
using UnityEngine;

namespace ANovel
{
	public class ComponentPool<T> : IDisposable where T : Component
	{
		Transform m_Root;
		Queue<T> m_Pool = new Queue<T>();
		List<T> m_List = new List<T>();

		public ComponentPool(Transform root)
		{
			m_Root = root;
		}

		public virtual T Get()
		{
			if (m_Pool.Count > 0)
			{
				var source = m_Pool.Dequeue();
				source.gameObject.SetActive(true);
				return source;
			}
			else
			{
				var source = new GameObject(typeof(T).Name).AddComponent<T>();
				source.transform.SetParent(m_Root);
				m_List.Add(source);
				OnCreate(source);
				return source;
			}
		}

		protected virtual void OnCreate(T obj) { }

		public virtual void Return(T item)
		{
			if (item.transform.parent != m_Root)
			{
				item.transform.SetParent(m_Root);
			}
			item.gameObject.SetActive(false);
			m_Pool.Enqueue(item);
		}

		public void Dispose()
		{
			var list = m_List.ToArray();
			m_List.Clear();
			foreach (var item in list)
			{
				GameObject.Destroy(item.gameObject);
			}
		}


	}

}