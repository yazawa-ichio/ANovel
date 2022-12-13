namespace ANovel.Core
{

	public class HistoryLog : IHistoryLog
	{
		public string FilePath;
		public BlockLabelInfo LabelInfo;
		public TextBlock Text;
		public EnvDataDiff Diff;
		public EnvDataSnapshot ExtensionSave;
		public EnvDataSnapshot PlayingSave;

		EnvData m_Extension;
		EnvData m_PlayingEnvData;

		BlockLabelInfo IHistoryLog.LabelInfo => LabelInfo;

		TextBlock IHistoryLog.Text => Text;

		IEnvDataHolder IHistoryLog.Extension
		{
			get
			{
				if (m_Extension == null)
				{
					m_Extension = new EnvData();
					if (ExtensionSave != null)
					{
						m_Extension.Load(ExtensionSave);
					}
				}
				return m_Extension;
			}
		}

		IEnvDataHolder IHistoryLog.PlayingEnvData
		{
			get
			{
				if (m_PlayingEnvData == null)
				{
					m_PlayingEnvData = new EnvData();
					if (m_PlayingEnvData != null)
					{
						m_PlayingEnvData.Load(PlayingSave);
					}
				}
				return m_PlayingEnvData;
			}
		}

		[UnityEngine.Scripting.Preserve]
		public HistoryLog() { }

		public HistoryLog(Block block, EnvDataDiff diff, EnvData extension, EnvDataSnapshot playing)
		{
			FilePath = block.FilePath;
			Text = block.Text?.Clone();
			LabelInfo = block.LabelInfo;
			Diff = diff;
			m_Extension = extension;
			ExtensionSave = extension.Save();
			PlayingSave = playing;
		}

	}
}