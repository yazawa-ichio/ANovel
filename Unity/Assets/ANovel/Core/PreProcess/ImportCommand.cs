namespace ANovel.Core
{
	[TagName("import")]
	public sealed class ImportCommand : PreProcess
	{
		[Argument(Required = true)]
		public string Path { get; private set; }

		PreProcessResult m_Import;

		public void Import(PreProcessResult result)
		{
			m_Import = result;
		}

		public override void Result(PreProcessResult result)
		{
			result.DependMacros.Add(m_Import.MacroDefine);
			result.Meta.Depend(m_Import.Meta);
			result.Converters.AddRange(m_Import.Converters);
		}

	}
}