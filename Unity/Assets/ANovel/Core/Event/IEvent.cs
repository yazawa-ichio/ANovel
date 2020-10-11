namespace ANovel.Core
{
	public interface IEvent<TValue>
	{
		object Owner { get; }
		bool IsSameAction(object action);
		void Invoke(TValue value);
	}
}