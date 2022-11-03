using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ANovel.Core.Tests
{

	public class LocalizeTests
	{

		[Test]
		public void ローカライズテキスト()
		{
			TestLocalize("LocalizeIndexTest", "jp", new string[] { "一", "二", "三", "四", "七", "八", "九", "三", "四", "五" });
			TestLocalize("LocalizeIndexTest", "en", new string[] { "one", "two", "three", "four", "seven", "eight", "nine", "three", "four", "five" });
			TestLocalize("LocalizeKeyTest", "jp", new string[] { "一", "二", "三", "四", "七", "八", "九", "三", "四", "五" });
			TestLocalize("LocalizeKeyTest", "en", new string[] { "one", "two", "three", "four", "seven", "eight", "nine", "three", "four", "five" });
		}

		void TestLocalize(string file, string lang, string[] texts)
		{
			var reader = new BlockReader(new ResourcesScenarioLoader("TestScenario"), new string[] { "TEST" });
			var text = new Text(lang);
			var conductor = new Conductor(reader, null)
			{
				Text = text
			};
			conductor.OnLoad = text.OnLoad;
			conductor.Run(file, null, CancellationToken.None).Wait();
			conductor.Update();
			while (conductor.TryNext())
			{
				conductor.Update();
			}
			Assert.AreEqual(texts.Length, text.List.Count);
			for (int i = 0; i < text.List.Count; i++)
			{
				Assert.AreEqual(texts[i], text.List[i].Text);
			}
		}


		class Text : ITextProcessor
		{
			public ServiceContainer Container { get; private set; }

			public TextBlock TextBlock { get; private set; }

			public bool Next { get; set; } = true;

			public bool IsProcessing => true;

			public List<LocalizeTextEnvData> List = new List<LocalizeTextEnvData>();

			string m_Lang;

			public Text(string lang)
			{
				m_Lang = lang;
			}

			public void Init(ServiceContainer container)
			{
				Container = container;
			}

			public void Set(TextBlock text, IEnvDataHolder data, IMetaData meta)
			{
				TextBlock = text?.Clone();
				UnityEngine.Debug.Log(text.Get());
				Assert.IsTrue(meta.TryGetSingle<LocalizeMetaData>(out var localize));
				Assert.AreEqual(2, data.GetAll<LocalizeTextEnvData>().Count());
				foreach (var kvp in data.GetAll<LocalizeTextEnvData>())
				{
					if (kvp.Value.Lang == m_Lang)
					{
						List.Add(kvp.Value);
					}
				}
			}

			public Task OnLoad(Block block, IEnvDataHolder data)
			{
				TextBlock = block?.Text?.Clone();
				return Task.FromResult(true);
			}

			public bool TryNext()
			{
				return true;
			}
			public void Clear()
			{
				TextBlock = null;
			}

			public string GetLocalizeKey(TextBlock text)
			{
				return text.Get();
			}
		}
	}
}