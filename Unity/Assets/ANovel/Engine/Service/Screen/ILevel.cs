using UnityEngine;

namespace ANovel.Engine
{
	public interface ILevel
	{
		IScreenId ScreenId { get; }
		string Name { get; }
		int Order { get; }
		RectTransform Root { get; }
	}
}