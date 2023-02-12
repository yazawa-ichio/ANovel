using System.Collections.Generic;

namespace ANovel.Actions
{
	public interface IActionParam : IEnvValue
	{
		Millisecond Time { get; }
		Millisecond Start { get; }
		IEnumerable<IActionPlaying> CreateActions(object target);
	}
}