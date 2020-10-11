using NUnit.Framework;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ANovel.Core.Tests
{

	public class ResourceTests
	{

		[Test]
		public void キャッシュテスト()
		{
			var loader = new DummyLoader();
			var cache = new ResourceCache(loader);
			var handle = cache.Load<TestAsset1>("TestAsset1");
			Assert.AreEqual(1, loader.LoadCount, "ロードされる");
			Assert.IsNotNull(handle.GetAsync().Result);

			using (cache.Load<TestAsset1>("TestAsset1"))
			{
				Assert.AreEqual(1, loader.LoadCount, "キャッシュ済みなので増えない");
			}

			Assert.IsNotNull(cache.Get<TestAsset1>("TestAsset1"));

			cache.ReleaseUnused();
			using (cache.Load<TestAsset1>("TestAsset1"))
			{
				Assert.AreEqual(1, loader.LoadCount, "ハンドルがあるので解放されない");
			}

			handle.Dispose();
			Assert.IsTrue(handle.Disposed, "解放済みか分かる");

			cache.ReleaseUnused();

			handle = cache.Load<TestAsset1>("TestAsset1");
			{
				Assert.AreEqual(2, loader.LoadCount, "リリース出来てるので再度ロードされる");
			}

			cache.Dispose();
			handle.Dispose();
		}

		[Test, Timeout(100000)]
		public void ハンドル回収テスト()
		{
			var loader = new DummyLoader();
			var cache = new ResourceCache(loader);
			cache.Load<TestAsset1>("TestAsset1");
			Assert.AreEqual(1, loader.LoadCount, "キャッシュがない場合はロードカウントが増える");
			int count = 0;
			while (count < 10)
			{
				Thread.Sleep(100);
				System.GC.Collect();
				cache.ReleaseUnused();
				count++;
			}
			cache.Load<TestAsset1>("TestAsset1");
			Assert.AreEqual(2, loader.LoadCount, "HandleがGCで回収されても解放される");
		}

		[UnityTest, Timeout(3000)]
		public IEnumerator キャッシュエラーテスト() => Async(async () =>
		{
			var loader = new DummyLoader();
			var cache = new ResourceCache(loader);
			Assert.Throws<CacheException>(() =>
			{
				cache.Get<TestAsset1>("TestAsset1");
			}, "キャッシュしていないのに取得している");
			cache.LoadRaw<Test1>("Test");
			Assert.Throws<CacheException>(() =>
			{
				cache.LoadRaw<Test2>("Test");
			}, "異なる型でロードしている");

			Assert.Throws<CacheException>(() =>
			{
				cache.Load<TestAsset2>("Test");
			}, "異なる型でロードしている");

			loader.Delay = 1000;
			var handle = cache.Load<TestAsset1>("TestAsset1");
			var task = handle.GetAsync();
			Assert.Throws<CacheException>(() =>
			{
				_ = handle.Value;
			}, "ロード前にキャッシュアクセス");
			handle.Dispose();
			cache.ReleaseUnused();
			try
			{
				await task;
				Assert.Fail("キャンセル出来る");
			}
			catch (OperationCanceledException) { }
			Assert.Throws<ObjectDisposedException>(() =>
			{
				_ = handle.Value;
			}, "破棄したハンドルへのアクセス");
		});

		[UnityTest, Timeout(3000)]
		public IEnumerator キャッシュスコープテスト() => Async(async () =>
		{
			var loader = new DummyLoader();
			loader.Delay = 200;
			var cache = new ResourceCache(loader);
			var scope = new PreLoadScope(cache);
			scope.Load<TestAsset1>("TestAsset1");
			scope.LoadRaw<Test1>("Test1");

			Assert.IsFalse(scope.IsLoaded);

			var task = scope.WaitComplete();
			await Task.WhenAny(task, Task.Delay(1000));

			Assert.IsTrue(scope.IsLoaded, "ロード待ち");
			Assert.NotNull(cache.Get<TestAsset1>("TestAsset1"));
			Assert.NotNull(cache.Get<Test1>("Test1"));

			Assert.AreEqual(2, loader.LoadCount, "ロード数があっている");

			scope.Dispose();
			cache.ReleaseUnused();

			scope.Load<TestAsset1>("TestAsset1");
			Assert.AreEqual(3, loader.LoadCount, "ロード数があっている");

		});

		IEnumerator Async(Func<Task> func)
		{
			var task = func();
			while (!task.IsCompleted)
			{
				yield return null;
			}
			if (task.IsFaulted)
			{
				throw task.Exception;
			}
		}


		class TestAsset1 : ScriptableObject { }
		class TestAsset2 : ScriptableObject { }

		class Test1 { }
		class Test2 { }

		class DummyLoader : IResourceLoader
		{

			public int Delay = 0;

			public int LoadCount = 0;

			public async Task<T> Load<T>(string path, CancellationToken token) where T : Object
			{
				LoadCount++;
				if (Delay > 0)
				{
					await Task.Delay(Delay, token);
				}
				token.ThrowIfCancellationRequested();
				return ScriptableObject.CreateInstance(typeof(T)) as T;
			}

			public Task<T> LoadRaw<T>(string path, CancellationToken token)
			{
				LoadCount++;
				return Task.FromResult(System.Activator.CreateInstance<T>());
			}

		}
	}

}