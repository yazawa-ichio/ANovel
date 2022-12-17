namespace ANovel.Engine
{
	public struct ActionTimeEnvData : IEnvValue, IScreenChildEnvData, IBlockTemporaryEnvData
	{
		bool IBlockTemporaryEnvData.Delete => true;

		public float Time;
	}
}