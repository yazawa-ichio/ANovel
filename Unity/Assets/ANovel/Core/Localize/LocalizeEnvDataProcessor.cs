namespace ANovel.Core
{
	internal class LocalizeEnvDataProcessor : IEnvDataCustomProcessor
	{
		public int Priority => 1;

		public void PostJump(IMetaData meta, IEnvData data)
		{
			data.DeleteAll<LocalizeTextEnvData>();
			data.DeleteSingle<LocalizeIndexEnvData>();
		}

		public void PostUpdate(EnvDataUpdateParam param)
		{
		}

		public void PreUpdate(EnvDataUpdateParam param)
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
			if (lang.UseKey)
			{
				foreach (var text in lang.GetTexts(index.Index))
				{
					if (text.Default)
					{
						data.SetSingle(text);
					}
					data.Set(text.Lang, text);
				}
			}
			else
			{
				foreach (var text in lang.GetTexts(index.Index))
				{
					if (text.Default)
					{
						data.SetSingle(text);
					}
					data.Set(text.Lang, text);
				}
			}


		}
	}
}