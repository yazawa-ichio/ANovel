using System.Collections.Generic;

namespace ANovel.Core
{
	public readonly struct EnvDataUpdateParam
	{
		public readonly IEnvData Data;
		public TextBlock Text => m_Block.Text;
		public IMetaData Meta => m_Block.Meta;
		public bool HasStopCommand => m_Block.StopCommand != null;
		public bool SkipHistory => m_Block.SkipHistory;
		public bool ClearCurrentText => m_Block.ClearCurrentText;

		readonly Block m_Block;
		readonly List<ICommand> m_Commands;

		public EnvDataUpdateParam(IEnvData data, Block block, List<ICommand> commands)
		{
			Data = data;
			m_Block = block;
			m_Commands = commands;
		}

		public void AddCommand(ICommand command)
		{
			m_Commands.Add(command);
		}

	}

}