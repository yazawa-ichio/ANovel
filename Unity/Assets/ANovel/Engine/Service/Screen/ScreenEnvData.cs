namespace ANovel.Engine
{
	public interface IScreenChildEnvData
	{
	}

	public struct ScreenOrderEnvData : IScreenChildEnvData
	{
		public long AutoOrder;

		public static long GenOrder(IEnvData data)
		{
			data.TryGetSingle<ScreenOrderEnvData>(out var order);
			order.AutoOrder += 2;
			data.SetSingle(order);
			return order.AutoOrder;
		}

	}
}