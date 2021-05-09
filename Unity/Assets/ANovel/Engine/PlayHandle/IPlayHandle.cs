using System;

namespace ANovel
{

	public interface IPlayHandle : IDisposable
	{
		bool IsPlaying { get; }
		event Action OnComplete;
	}

}