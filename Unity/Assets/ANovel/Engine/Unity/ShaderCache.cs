using System.Collections.Generic;
using UnityEngine;

namespace ANovel
{

	public static class ShaderCache
	{
		static Dictionary<string, Shader> s_Dic = new Dictionary<string, Shader>();

		public static Shader Get(string name)
		{
			// Unloadされている可能性があるのでnullチェックする
			if (!s_Dic.TryGetValue(name, out var shader) || shader == null)
			{
				s_Dic[name] = shader = Shader.Find(name);
			}
			return shader;
		}

		public static void Clear()
		{
			s_Dic.Clear();
		}

	}

}