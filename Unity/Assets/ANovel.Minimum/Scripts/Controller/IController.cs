namespace ANovel.Minimum
{
	public interface IController
	{
		System.Type ControllerType { get; }
		float DeltaTime { get; }
		void Setup(Config config);
		void Pause();
		void Resume();
		void Clear();
	}
}