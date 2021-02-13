using UnityEngine;

namespace ANovel.Service
{
	public interface ILevel
	{
		IScreenId ScreenId { get; }
		string Name { get; }
		int Order { get; }
		RectTransform Root { get; }
	}
}