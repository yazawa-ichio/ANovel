namespace ANovel.Core
{
	public abstract class PreProcess : Tag
	{
		public virtual bool HeaderOnly => true;
		public virtual void Result(PreProcessor.Result result) { }
	}

}