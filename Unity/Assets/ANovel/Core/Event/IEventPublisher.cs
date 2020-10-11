namespace ANovel.Core
{
	public interface IEventPublisher
	{
		void Unsubscribe(object owner);
		void Refresh();
	}
}