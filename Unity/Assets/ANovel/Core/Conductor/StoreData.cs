using ANovel.Core;

namespace ANovel
{
	public class StoreData
	{
		public BlockLabelInfo Label;
		public EnvDataSnapshot Snapshot;
		public EnvDataSnapshot PlayingSave;
		public HistoryLog[] Logs;

		public byte[] Save() => Packer.Pack(this);

		public static StoreData Load(byte[] buf) => Packer.Unpack<StoreData>(buf);

	}
}