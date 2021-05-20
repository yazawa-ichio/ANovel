using ANovel.Core;
using System;
using System.Collections.Generic;

namespace ANovel
{
	public interface IHistory
	{
		int MaxLogCount { get; set; }

		bool CanRollback { get; set; }

		BlockLabelInfo CurrentLabel { get; }

		event Action<HistoryAddEvent> OnAdd;

		IEnumerable<IHistoryLog> GetLogs();

		IHistoryLog GetCurrentLog();
	}
}