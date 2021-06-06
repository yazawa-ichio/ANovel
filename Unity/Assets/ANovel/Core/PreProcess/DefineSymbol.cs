namespace ANovel.Core
{
	public interface IDefineSymbol
	{
		void Result(PreProcessResult result);
	}

	[TagName("define_symbol")]
	public class DefineSymbol : PreProcess, IDefineSymbol
	{
		[Argument(Required = true)]
		string m_Name = default;

		public override void Result(PreProcessResult result)
		{
			base.Result(result);
			if (!result.Symbols.Contains(m_Name))
			{
				result.Symbols.Add(m_Name);
			}
		}
	}

}