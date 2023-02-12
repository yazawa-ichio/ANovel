namespace ANovel.Actions
{
	public interface IActionPlaying
	{
		Millisecond Time { get; }
		Millisecond Start { get; }

		void Begin();
		void End();
		void Update(float delatTime);
	}
}