#if ANOVEL_LOCAL_FILE_TEST
using NUnit.Framework;
using System.IO;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace ANovel.Core.Tests
{
	public class FileLoadTests
	{

		[Test]
		public void ファイルロードテスト()
		{
			var root = Path.Combine(Application.dataPath, "ANovel/Tests/TestData~");
			{
				using (var loader = new LocalFileLoader(root))
				{
					Assert.IsNotNull(loader.Load("ImportMacroTest.anovel").Result);
				}
			}
			{
				using (var loader = new LocalFileLoader(null))
				{
					var path = Path.Combine(root, "ImportMacroTest.anovel");
					Assert.IsNotNull(loader.Load(path).Result);
				}
			}
		}

	}
}
#endif