using ANovel.Core;
using UnityEngine.Scripting;

namespace ANovel
{
	public class EventSubscribeAttribute : PreserveAttribute
	{
		public readonly string Name;

		public EventSubscribeAttribute(object name)
		{
			Name = EventNameToStrConverter.ToStr(name);
		}
	}

}