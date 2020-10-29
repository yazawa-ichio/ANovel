namespace ANovel.Minimum
{
	public abstract class MinimumCommand : Command
	{
		protected Config Config => Get<Config>();

		protected ISoundController Sound => Get<ISoundController>();

		protected IFadeController Fade => Get<IFadeController>();

		protected IImageController Image => Get<IImageController>();

	}

}