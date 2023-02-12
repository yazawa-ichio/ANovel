namespace ANovel.Core
{
	internal class LocalizeEnvDataProcessor : IEnvDataCustomProcessor
	{
		public int Priority => 1;

		ServiceContainer m_Container;

		ITextProcessor Processor
		{
			get
			{
				m_Container.TryGet<ITextProcessor>(out var text);
				return text;
			}
		}

		public LocalizeEnvDataProcessor(ServiceContainer container)
		{
			m_Container = container;
		}

		public void PostJump(IMetaData meta, IEnvData data)
		{
			data.DeleteAll<LocalizeTextEnvData>();
			data.DeleteSingle<LocalizeIndexEnvData>();
		}

		public void PostUpdate(EnvDataUpdateParam param)
		{
			if (param.Text == null)
			{
				param.Data.DeleteAll<LocalizeTextEnvData>();
				return;
			}
			if (!param.Meta.TryGetSingle<LocalizeMetaData>(out var lang))
			{
				return;
			}
			var data = param.Data;
			if (lang.UseKey)
			{
				foreach (var text in lang.GetTexts(Processor.GetLocalizeKey(param.Text)))
				{
					data.Set(text.Lang, text);
				}
			}
			else if (data.TryGetSingle<LocalizeIndexEnvData>(out var index))
			{
				foreach (var text in lang.GetTexts(index.Index))
				{
					data.Set(text.Lang, text);
				}
			}
		}

		public void PreUpdate(EnvDataUpdateParam param)
		{
			if (param.Text == null)
			{
				param.Data.DeleteAll<LocalizeTextEnvData>();
				return;
			}
			if (!param.Meta.TryGetSingle<LocalizeMetaData>(out var lang) || lang.UseKey)
			{
				return;
			}
			var data = param.Data;
			if (data.TryGetSingle<LocalizeIndexEnvData>(out var index))
			{
				index.Index++;
				data.SetSingle(index);
			}
			else
			{
				index = new LocalizeIndexEnvData();
				data.SetSingle(index);
			}
		}
	}
}