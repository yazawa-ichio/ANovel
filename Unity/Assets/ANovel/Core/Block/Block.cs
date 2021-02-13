using ANovel.Commands;
using System;
using System.Collections.Generic;

namespace ANovel.Core
{
	public class Block : IDisposable
	{
		public string FilePath { get; private set; }

		public BlockLabelInfo LabelInfo;

		public List<ICommand> Commands { get; private set; }

		public TextBlock Text { get; private set; }

		public IStopCommand StopCommand { get; private set; }

		public bool SkipHistory { get; private set; }

		public bool ClearCurrentText => StopCommand == null || StopCommand.ClearText;

		public Block(string filePath, in BlockLabelInfo label, List<ICommand> commands, TextBlock text)
		{
			FilePath = filePath;
			LabelInfo = label;
			Commands = ListPool<ICommand>.Pop();
			foreach (var cmd in commands)
			{
				Commands.Add(cmd);
				if (cmd is IStopCommand stop)
				{
					StopCommand = stop;
				}
				if (cmd is ISkipHistoryCommand history)
				{
					SkipHistory = history.SkipHistory;
				}
			}
			Text = text;
		}

		public void Dispose()
		{
			LabelInfo = default;
			ListPool<ICommand>.Push(Commands);
			Commands = null;
			Text?.Dispose();
			Text = null;
			GC.SuppressFinalize(this);
		}

	}

}