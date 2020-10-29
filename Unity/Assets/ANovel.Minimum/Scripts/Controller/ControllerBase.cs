using UnityEngine;

namespace ANovel.Minimum
{
	public abstract class ControllerBase : MonoBehaviour, IController
	{
		protected Config Config { get; private set; }

		public abstract System.Type ControllerType { get; }

		public bool IsPause { get; private set; }

		public float DeltaTime => IsPause ? 0 : Time.deltaTime;

		public void Setup(Config config)
		{
			Config = config;
			OnSetup();
		}

		protected virtual void OnSetup() { }

		public void Pause()
		{
			IsPause = true;
			OnPause();
		}

		public void Resume()
		{
			IsPause = false;
			OnResume();
		}

		protected virtual void OnPause() { }

		protected virtual void OnResume() { }

		public virtual void Clear() { }

	}
}