﻿using ANovel.Commands;
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

		public IMetaData Meta { get; private set; }

		public Block(string filePath, in BlockLabelInfo label, List<ICommand> commands, TextBlock text, IMetaData meta)
		{
			FilePath = filePath;
			LabelInfo = label;
			Commands = ListPool<ICommand>.Pop();
			int index = 0;
			while (index < commands.Count)
			{
				ICommand cmd = commands[index++];
				if (cmd is IScopeCommand batch)
				{
					bool batchEnd = false;
					batch.BeginAddCommand();
					while (index < commands.Count)
					{
						if (batch.AddCommand(commands[index++]))
						{
							batchEnd = true;
							break;
						}
					}
					if (!batchEnd)
					{
						throw new Exception("not found batch end");
					}
					batch.EndAddCommand();
				}
				Commands.Add(cmd);
				if (cmd is IStopCommand stop)
				{
					StopCommand = stop;
				}
				if (cmd is ISkipHistoryCommand history)
				{
					SkipHistory |= history.SkipHistory;
				}
			}
			Text = text;
			Meta = meta;
		}

		public void Dispose()
		{
			LabelInfo = default;
			ListPool<ICommand>.Push(Commands);
			Commands = null;
			Text?.Dispose();
			Text = null;
		}

	}

}