namespace ANovel
{
	//　演出毎にもうちょっと細分化したい

	public interface IEngineTime
	{
		float DeltaTime { get; }
		float Time { get; }
	}

	public class EngineTime : IEngineTime
	{
		public float DeltaTime => UnityEngine.Time.deltaTime;

		public float Time => UnityEngine.Time.time;
	}

}