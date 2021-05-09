using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public class PreLoadScope : IPreLoader, IDisposable
	{
		ResourceCache m_Owner;
		List<ICacheHandle> m_Handles = new List<ICacheHandle>();

		public bool IsLoaded
		{
			get
			{
				foreach (var h in m_Handles)
				{
					if (!h.IsLoaded)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool IsDone
		{
			get
			{
				foreach (var h in m_Handles)
				{
					if (!h.IsDone)
					{
						return false;
					}
				}
				return true;
			}
		}

		public PreLoadScope(ResourceCache owner)
		{
			m_Owner = owner;
		}

		public void CheckError()
		{
			foreach (var h in m_Handles)
			{
				h.CheckError();
			}
		}

#if UNITY_5_3_OR_NEWER
		public void Load<T>(string path) where T : UnityEngine.Object
		{
			m_Handles.Add(m_Owner.Load<T>(path));
		}
#endif

		public void LoadRaw<T>(string path) where T : class
		{
			m_Handles.Add(m_Owner.LoadRaw<T>(path));
		}

		public Task WaitComplete()
		{
			return Task.WhenAll(m_Handles.Select(x => x.GetAsync()).ToArray());
		}

		public void Dispose()
		{
			foreach (var handler in m_Handles)
			{
				handler.Dispose();
			}
			m_Handles.Clear();
		}

	}

}