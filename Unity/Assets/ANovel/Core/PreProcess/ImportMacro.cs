namespace ANovel.Core
{
	[TagName("importmacro", LineType.PreProcess)]
	public class ImportMacro : PreProcess, IImportPreProcess
	{
		[TagField(Required = true)]
		public string Path { get; private set; }

		PreProcessor.Result m_Import;

		public void Import(PreProcessor.Result result)
		{
			m_Import = result;
		}

		public override void Result(PreProcessor.Result result)
		{
			result.DependMacros.Add(m_Import.MacroDefine);
		}

	}
}