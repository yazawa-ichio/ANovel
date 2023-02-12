using System.Threading.Tasks;

namespace ANovel.Core
{
	public class SimpleCacheHandle<T> : ICacheHandle<T> where T : class
	{
		public T Value { get; private set; }

		public bool IsLoaded => true;

		public bool IsDone => true;

		public bool Disposed { get; private set; }

		public SimpleCacheHandle(T value)
		{
			Value = value;
		}

		public void Dispose()
		{
			Disposed = true;
		}

		public ICacheHandle<T> Duplicate()
		{
			return new SimpleCacheHandle<T>(Value);
		}

		public Task<T> GetAsync()
		{
			return Task.FromResult(Value);
		}

		Task ICacheHandle.GetAsync()
		{
			return GetAsync();
		}

		public void CheckError()
		{
		}

	}
}