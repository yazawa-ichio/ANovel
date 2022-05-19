namespace ANovel.Core
{

	public interface IEvaluator
	{
		IVariableContainer Variables { get; }
		IVariableContainer GlobalVariables { get; }
		object Eval(string value);
		object Eval(string value, LineData? data);
	}
}