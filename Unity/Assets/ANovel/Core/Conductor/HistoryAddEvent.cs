namespace ANovel.Core
{
	public class HistoryAddEvent
	{
		public readonly TextBlock Text;
		public readonly BlockLabelInfo LabelInfo;
		public readonly IEnvDataHolder EnvData;
		public readonly IEnvData Extension;

		public HistoryAddEvent(TextBlock text, BlockLabelInfo labelInfo, IEnvDataHolder envData, IEnvData extension)
		{
			Text = text;
			LabelInfo = labelInfo;
			EnvData = envData;
			Extension = extension;
		}
	}
}