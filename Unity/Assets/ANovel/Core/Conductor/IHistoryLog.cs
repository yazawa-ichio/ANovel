namespace ANovel
{
	public interface IHistoryLog
	{
		BlockLabelInfo LabelInfo { get; }
		TextBlock Text { get; }
		IEnvDataHolder Extension { get; }
	}
}