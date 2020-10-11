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

		public bool IsLoaded => !m_Handles.Any(x => !x.IsLoaded);

		public PreLoadScope(ResourceCache owner)
		{
			m_Owner = owner;
		}

#if UNITY_5_3_OR_NEWER
		public void Load<T>(string path) where T : UnityEngine.Object
		{
			m_Handles.Add(m_Owner.Load<T>(path));
		}
#endif

		public void LoadRaw<T>(string path)
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