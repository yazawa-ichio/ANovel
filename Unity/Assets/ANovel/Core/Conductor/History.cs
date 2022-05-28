
using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{

	public class History : IHistory
	{
		public int MaxLogCount { get; set; } = 128;

		public bool CanRollback { get; set; } = true;

		public event Action<HistoryAddEvent> OnAdd;

		LinkedList<HistoryLog> m_LogList = new LinkedList<HistoryLog>();

		public BlockLabelInfo CurrentLabel
		{
			get
			{
				if (m_LogList.Count > 0)
				{
					return m_LogList.Last.Value.LabelInfo;
				}
				return default;
			}
		}

		public void Add(EnvData envData, Block block, EnvDataDiff diff)
		{
			if (block.SkipHistory)
			{
				return;
			}
			var extension = new EnvData();
			extension.Load(envData.SaveByInterface<IHistorySaveEnvData>());
			var e = new HistoryAddEvent(block.Text, block.LabelInfo, envData, extension);
			// 必要であればExtensionに情報を入れる
			OnAdd?.Invoke(e);
			var _diff = CanRollback ? diff : null;
			var log = new HistoryLog(block, _diff, extension);

			if (m_LogList.Count >= MaxLogCount)
			{
				m_LogList.RemoveFirst();
			}
			m_LogList.AddLast(log);
		}

		public bool TryBack(int num, out EnvDataDiff[] diff)
		{
			using (ListPool<EnvDataDiff>.Use(out var list))
			{
				if (m_LogList.Count <= num)
				{
					diff = null;
					return false;
				}
				for (int i = 0; i < num; i++)
				{
					var item = m_LogList.Last;
					m_LogList.Remove(item);
					list.Add(item.Value.Diff);
				}
				diff = list.ToArray();
				return true;
			}
		}

		public void Clear()
		{
			m_LogList.Clear();
		}

		public void Load(HistoryLog[] logs)
		{
			m_LogList = new LinkedList<HistoryLog>(logs);
		}

		public HistoryLog[] Save()
		{
			return m_LogList.ToArray();
		}

		public IEnumerable<IHistoryLog> GetLogs()
		{
			return m_LogList;
		}


		public IHistoryLog GetCurrentLog()
		{
			if (m_LogList.Count == 0)
			{
				return null;
			}
			return m_LogList.Last.Value;
		}

		public int GetBackNum(IHistoryLog log)
		{
			int i = 0;
			foreach (var item in m_LogList.Reverse())
			{
				if (item == log)
				{
					return i;
				}
				i++;
			}
			return 0;
		}
	}
}