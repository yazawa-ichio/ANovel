using ANovel.Core;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Actions
{
	public struct ActionData : IActionData
	{
		public Millisecond Total;

		public EnvDataSnapshot Data;

		public IEnumerable<IActionParam> GetParams()
		{
			var data = new EnvData();
			data.Load(Data);
			foreach (var p in data.GetAllByInterface<IActionParam>().OrderBy(x => int.Parse(x.Key)).Select(x => x.Value))
			{
				yield return p;
			}
		}
	}
}