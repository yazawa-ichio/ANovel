using System;

namespace ANovel.Core
{
	public class CacheException : Exception
	{
		public string Path { get; private set; }

		public CacheException(string path, string message) : base(GetMessage(path, message))
		{
			Path = path;
		}

		static string GetMessage(string path, string message)
		{
#if DEBUG || UNITY_EDITOR
			return $"{message} Path:{path}";
#else
			return $"{message}";
#endif
		}

	}
}