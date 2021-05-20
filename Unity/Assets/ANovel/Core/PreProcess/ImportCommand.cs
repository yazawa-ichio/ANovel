namespace ANovel.Core
{
	[TagName("import")]
	public class ImportCommand : PreProcess, IImportPreProcess
	{
		[Argument(Required = true)]
		public string Path { get; private set; }

		PreProcessor.Result m_Import;

		public void Import(PreProcessor.Result result)
		{
			m_Import = result;
		}

		public override void Result(PreProcessor.Result result)
		{
			result.DependMacros.Add(m_Import.MacroDefine);
			result.Meta.Depend(m_Import.Meta);
			result.Converters.AddRange(m_Import.Converters);
		}

	}
}