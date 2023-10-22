using ANovel.Commands;

namespace ANovel.Core
{
	[TagName("localize_data")]
	public class LocalizeDataCommand : PreProcess, IImportText
	{
		public enum ImportType
		{
			Auto,
			Csv,
			Tsv,
			Json,
		}

		[Argument(Required = true)]
		public string Path { get; private set; }
		[Argument]
		ImportType m_Type = ImportType.Auto;
		[Argument]
		bool m_UseKey;
		[Argument]
		bool m_UseDefault = true;

		LocalizeData m_Data;

		public void Import(string text)
		{
			switch (m_Type)
			{
				case ImportType.Csv:
					m_Data = LocalizeData.Create(text, tab: false);
					break;
				case ImportType.Tsv:
					m_Data = LocalizeData.Create(text, tab: true);
					break;
				case ImportType.Json:
					m_Data = UnityEngine.JsonUtility.FromJson<LocalizeData>(text);
					break;
				default:
					if (text.StartsWith("{"))
					{
						m_Data = UnityEngine.JsonUtility.FromJson<LocalizeData>(text);
					}
					else
					{
						m_Data = LocalizeData.Create(text, tab: false);
					}
					break;
			}
		}

		public override void Result(PreProcessResult result)
		{
			result.Meta.SetSingle(new LocalizeMetaData(m_Data, m_UseKey, m_UseDefault));
		}
	}


	[TagName("localize_index")]
	public class LocalizeDataIndexCommand : SystemCommand
	{
		[Argument(Required = true)]
		int m_Index;

		protected override void UpdateEnvData(IEnvData data)
		{
			if (Meta.TryGetSingle<LocalizeMetaData>(out var lang))
			{
				data.TryGetSingle<LocalizeIndexEnvData>(out var envData);
				envData.Index = m_Index;
				data.SetSingle(envData);
			}
		}
	}

	[TagName("localize_key")]
	public class LocalizeKeyCommand : SystemCommand
	{
		[Argument(Required = true)]
		string m_Key;

		protected override void UpdateEnvData(IEnvData data)
		{
			if (Meta.TryGetSingle<LocalizeMetaData>(out var lang))
			{
				data.TryGetSingle<LocalizeIndexEnvData>(out var envData);
				envData.Index = lang.GetKeyIndex(m_Key);
				data.SetSingle(envData);
			}
		}
	}

}