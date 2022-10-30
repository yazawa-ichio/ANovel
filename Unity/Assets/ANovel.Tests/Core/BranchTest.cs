using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel.Core.Tests
{
	public class BranchTest
	{

		[Test]
		public void 比較テスト()
		{
			Assert.AreEqual(true, Run("&if condition=true\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(false, Run("&if condition=false\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(true, Run("&flag name=f&if flag=f\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(false, Run("&if flag=f\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(false, Run("&val name=f value=0\n&if flag=f\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(true, Run("&val name=f value=0\n&if has_val=f\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(false, Run("&if has_val=f\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(true, Run("&flag name=f global\n&if flag=f\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(false, Run("&if left=a right=b\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(true, Run("&if left=a right=b not\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(false, Run("&if left=b right=b not\n&flag name=ok").Variables.Has("ok"));


			Assert.AreEqual(true, Run("&if condition=1==1\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(true, Run("&if condition=1<=1\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(false, Run("&if condition=1<1\n&flag name=ok").Variables.Has("ok"));
			Assert.AreEqual(true, Run("&flag name=f\n&if condition=$\"{f}\"\n&flag name=ok").Variables.Has("ok"));
		}

		[Test]
		public void エラーテスト()
		{
			Error("&if condition=true\n*ErrorLabel&endif");
			Error("&elsif condition=true&endif");
			Error("&else");
			Error("&endif");
			Error("&if condition=val");
		}

		void Error(string value) => Error<LineDataException>(value);

		void Error<T>(string value) where T : Exception
		{
			Assert.Throws<T>(() =>
			{
				try
				{
					Run(value);
				}
				catch (Exception err)
				{
					Debug.LogWarning(err);
					throw;
				}
			});
		}

		IEvaluator Run(string value)
		{
			BlockReader reader = new BlockReader(new DummyLoader(value), new string[] { "TEST" });
			reader.Load("", CancellationToken.None).Wait();
			Assert.IsTrue(reader.TryRead(out var block));
			{
				foreach (var cmd in block.Commands.OfType<ICommand>())
				{
					cmd.Execute();
				}
			}
			return reader.Evaluator;
		}

		[Test]
		public void 分岐テスト()
		{
			var text = new Text();
			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			var conductor = new Conductor(reader, new ResourceLoader(""))
			{
				Text = text,
			};
			conductor.PreLoadCount = 0;
			conductor.OnError += (e) => Debug.LogException(e);
			conductor.Run("IfTest", "", CancellationToken.None).Wait();
			conductor.Update();
			for (int i = 0; i < 10000 && !conductor.IsStop; i++)
			{
				if (text.TextBlock != null)
				{
					Debug.Log("TextBlock:" + text.TextBlock.Get());
					Assert.AreEqual(text.TextBlock.Get(), conductor.Variables.Get("disp"), text.TextBlock.Get());
				}
				if (!conductor.TryNext())
				{
					conductor.Update();
				}
			}
			Assert.IsTrue(conductor.IsStop);
			Assert.AreEqual(3, conductor.Variables.Get("count"));
			Assert.AreEqual(true, conductor.Variables.Get("end"));
		}

		class Text : ITextProcessor
		{
			public ServiceContainer Container { get; private set; }

			public TextBlock TextBlock { get; private set; }

			public bool Next { get; set; } = true;

			public bool IsProcessing => true;

			public void Init(ServiceContainer container)
			{
				Container = container;
			}

			public void Set(TextBlock text, IEnvDataHolder data, IMetaData meta)
			{
				Debug.Log("Set:" + text.Get());
				TextBlock = text?.Clone();
			}

			public Task OnLoad(Block block, IEnvDataHolder data)
			{
				TextBlock = block?.Text?.Clone();
				return Task.FromResult(true);
			}

			public bool TryNext()
			{
				if (Next)
				{
					return true;
				}
				return false;
			}
			public void Clear()
			{
				TextBlock = null;
			}
		}

	}
}
