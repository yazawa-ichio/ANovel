#if ANOVEL_LOCAL_FILE_TEST
using NUnit.Framework;
using System.IO;
using System.Threading;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace ANovel.Core.Tests
{
	public class FileLoadTests
	{
#if UNITY_EDITOR
		[Test]
		public void ファイルロードテスト()
		{
			var root = Path.Combine(Application.dataPath, "ANovel.Tests/Resources/TestScenario");
			{
				using (var loader = new LocalScenarioLoader(root))
				{
					Assert.IsNotNull(loader.Load("ImportMacroTest.anovel", CancellationToken.None).Result);
				}
			}
			{
				using (var loader = new LocalScenarioLoader(null))
				{
					var path = Path.Combine(root, "ImportMacroTest.anovel");
					Assert.IsNotNull(loader.Load(path, CancellationToken.None).Result);
				}
			}
		}
#endif
	}
}
#endif