using System.Collections.Generic;

namespace ANovel.Core
{
	public class Block
	{
		public BlockLabelInfo LabelInfo;

		public List<Command> Commands { get; private set; } = new List<Command>();

		public TextBlock Text { get; private set; } = new TextBlock();

		public void Clear()
		{
			LabelInfo = default;
			Commands.Clear();
			Text.Clear();
		}

	}

}