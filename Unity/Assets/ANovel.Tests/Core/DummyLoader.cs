using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core.Tests
{
	public class DummyLoader : IScenarioLoader
	{
		string m_Text;
		public DummyLoader(string text)
		{
			m_Text = text;
		}

		public void Dispose() { }

		public Task<string> Load(string path, CancellationToken token)
		{
			return Task.FromResult(m_Text);
		}
	}

}