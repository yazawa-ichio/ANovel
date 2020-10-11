using System;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public interface IScenarioLoader : IDisposable
	{
		Task<string> Load(string path);
	}
}