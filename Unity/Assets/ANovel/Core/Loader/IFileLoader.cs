using System.Threading.Tasks;

namespace ANovel.Core
{
	public interface IFileLoader
	{
		Task<string> Load(string path);
	}
}