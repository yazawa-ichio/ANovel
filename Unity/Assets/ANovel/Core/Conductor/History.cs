
using System;
using System.Collections.Generic;
using System.Linq;

namespace ANovel.Core
{

	public class HistoryAddEvent
	{
		public readonly TextBlock Text;
		public readonly BlockLabelInfo LabelInfo;
		public readonly IEnvDataHolder EnvData;
		public readonly List<string> Extension;

		public HistoryAddEvent(TextBlock text, BlockLabelInfo labelInfo, IEnvDataHolder envData, List<string> extension)
		{
			Text = text;
			LabelInfo = labelInfo;
			EnvData = envData;
			Extension = extension;
		}
	}

	public class HistoryLog
	{
		public string FilePath;
		public BlockLabelInfo LabelInfo;
		public TextBlock Text;
		public EnvDataDiff Diff;
		public string[] Attribute;

		[UnityEngine.Scripting.Preserve]
		public HistoryLog() { }

		public HistoryLog(Block block, EnvDataDiff diff, string[] attribute)
		{
			FilePath = block.FilePath;
			Text = block.Text?.Clone();
			LabelInfo = block.LabelInfo;
			Diff = diff;
			Attribute = attribute;
		}

	}

	public class History
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

		public void Add(IEnvDataHolder envData, Block block, EnvDataDiff diff)
		{
			if (block.SkipHistory)
			{
				return;
			}
			using (ListPool<string>.Use(out var extension))
			{
				var e = new HistoryAddEvent(block.Text, block.LabelInfo, envData, extension);
				// 必要であればExtensionに情報を入れる
				OnAdd?.Invoke(e);
				var _extension = extension.Count == 0 ? Array.Empty<string>() : extension.ToArray();
				var _diff = CanRollback ? diff : null;
				var log = new HistoryLog(block, _diff, _extension);

				if (m_LogList.Count >= MaxLogCount)
				{
					m_LogList.RemoveFirst();
				}
				m_LogList.AddLast(log);
			}
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

	}
}