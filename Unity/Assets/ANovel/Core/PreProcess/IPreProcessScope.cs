namespace ANovel.Core
{
	public interface IPreProcessScope
	{
		bool IsEnd(PreProcess tag);
		void Add(in LineData data);
	}
}