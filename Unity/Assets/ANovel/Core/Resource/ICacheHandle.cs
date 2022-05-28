using System;
using System.Threading.Tasks;

namespace ANovel
{
	public interface ICacheHandle : IDisposable
	{
		bool IsLoaded { get; }
		bool Disposed { get; }
		bool IsDone { get; }

		Task GetAsync();
		void CheckError();
	}

	public interface ICacheHandle<T> : ICacheHandle where T : class
	{
		T Value { get; }
		new Task<T> GetAsync();
		ICacheHandle<T> Duplicate();
	}

}