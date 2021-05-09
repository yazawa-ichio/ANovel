namespace ANovel.Core
{
	public class BlockPreloadEntry
	{
		public Block Block { get; private set; }

		public PreLoadScope PreLoad { get; private set; }

		public EnvDataDiff Diff { get; private set; }

		public bool IsDone => PreLoad.IsDone && !CheckPreparing();

		public BlockPreloadEntry(Block block, PreLoadScope preLoad, EnvDataDiff diff)
		{
			Block = block;
			PreLoad = preLoad;
			Diff = diff;
		}

		bool CheckPreparing()
		{
			foreach (var cmd in Block.Commands)
			{
				if (!cmd.IsPrepared())
				{
					return true;
				}
			}
			return false;
		}

		public void CheckError()
		{
			PreLoad.CheckError();
		}

	}
}