namespace ANovel.Core
{
	public interface IDefineSymbol
	{
		void Result(PreProcessor.Result result);
	}

	[TagName("define", LineType.PreProcess)]
	public class DefineSymbol : PreProcess, IDefineSymbol
	{
		[TagField(Required = true)]
		string m_Name = default;

		public override void Result(PreProcessor.Result result)
		{
			base.Result(result);
			if (!result.Symbols.Contains(m_Name))
			{
				result.Symbols.Add(m_Name);
			}
		}
	}

}