using System;
using System.Threading;
using System.Threading.Tasks;

namespace ANovel
{
	public interface IScenarioLoader : IDisposable
	{
		Task<string> Load(string path, CancellationToken token);
	}
}