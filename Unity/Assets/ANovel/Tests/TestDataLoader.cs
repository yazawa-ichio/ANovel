using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core.Tests
{
	public class TestDataLoader : IScenarioLoader
	{
		public void Dispose() { }

		public Task<string> Load(string path, CancellationToken token)
		{
			var text = TestData.Get(path);
			return Task.FromResult(text);
		}
	}
}