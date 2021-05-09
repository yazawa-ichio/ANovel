using NUnit.Framework;

namespace ANovel.Core.Tests
{
	public class TestMetaData
	{
		[CommandField]
		string m_Name;
		public string Name => m_Name;
		[CommandField]
		string m_Value;
		public string Value => m_Value;
		[CommandField]
		public bool First;
	}

	public class TestSingleMetaData
	{
		public string Value;
	}

	[PreProcessName("test_meta_deine")]
	public class TestMetaDefineTest : PreProcess
	{
		[InjectParam]
		TestMetaData m_Data = new TestMetaData();

		public override void Result(PreProcessor.Result result)
		{
			if (string.IsNullOrEmpty(m_Data.Name))
			{
				Assert.AreEqual(m_Data.First, !result.Meta.TryGetSingle<TestSingleMetaData>(out _));
				if (m_Data.First)
				{
					result.Meta.AddSingle(new TestSingleMetaData()
					{
						Value = m_Data.Value
					});
				}
				else
				{
					result.Meta.SetSingle(new TestSingleMetaData()
					{
						Value = m_Data.Value
					});
				}
			}
			else
			{
				if (m_Data.First)
				{
					result.Meta.Add(m_Data.Name, m_Data);
				}
				else
				{
					result.Meta.Set(m_Data.Name, m_Data);
				}
			}
		}

	}
}