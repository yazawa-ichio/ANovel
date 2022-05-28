namespace ANovel.Core
{
	public interface IEvaluator
	{
		IVariableContainer Variables { get; }
		IVariableContainer GlobalVariables { get; }
		double Eval(string value, LineData? data);
		string ReplaceVariable(string value, LineData? data);
		bool Condition(string value, LineData? data);
	}
}