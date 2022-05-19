using System.Collections.Generic;
namespace ANovel.Core
{
	public interface IVariableContainer
	{
		IEnumerable<string> Keys { get; }
		bool Has(string name);
		void Set(string name, long value);
		void Set(string name, double value);
		void Set(string name, bool value);
		void Set(string name, string value);
		bool TryGetValue(string name, out object value);
		object Get(string name);
		void Delete(string name);
		void Clear();
	}
}