namespace ANovel.Core
{
	public interface IParamConverter
	{
		int Priority { get; }
		void Convert(TagParam param);
	}
}