using System.IO;
using System.Threading.Tasks;

namespace ANovel.Core
{
	public class LocalFileLoader : IScenarioLoader
	{
		string m_Root;

		public LocalFileLoader(string root)
		{
			m_Root = root;
		}

		string GetPath(string path)
		{
			if (string.IsNullOrEmpty(m_Root))
			{
				return path;
			}
			else
			{
				return m_Root + "/" + path;
			}
		}

		public Task<string> Load(string path)
		{
			var text = File.ReadAllText(GetPath(path));
			return Task.FromResult(text);
		}

		public void Dispose() { }

	}

}