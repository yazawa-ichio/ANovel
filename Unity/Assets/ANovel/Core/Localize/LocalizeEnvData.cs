namespace ANovel.Core
{
	internal struct LocalizeIndexEnvData : IEnvValue
	{
		public int Index;
	}

	public struct LocalizeTextEnvData : IEnvValue
	{
		public bool Default;
		public string Lang;
		public string Text;

		public LocalizedText CreateText()
		{
			return new LocalizedText(Text);
		}
	}
}