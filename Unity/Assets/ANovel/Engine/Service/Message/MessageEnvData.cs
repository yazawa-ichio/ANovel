using ANovel.Core;
using ANovel.Serialization;

namespace ANovel.Service
{
	public struct MessageEnvData : IDefaultValueSerialization, IHistorySaveEnvData
	{
		public bool IsDefault => Equals(default);

		public string Name;
		public string Chara;
		public string Message;
	}

	public struct MessageStatusEnvData : IDefaultValueSerialization
	{
		public bool IsDefault => Equals(default);

		public bool Hide;
	}

}