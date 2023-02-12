using ANovel.Core;

namespace ANovel.Actions
{

	public class ActionParamContext
	{
		public IMetaData Meta { get; private set; }

		public IEnvData EnvData { get; private set; }

		public Millisecond Time { get; private set; }

		EnvData m_List = new EnvData();
		int m_Index;
		Millisecond m_AddTime;

		public ActionParamContext(IEnvData data, IMetaData meta)
		{
			EnvData = data;
			Meta = meta;
		}

		public void Add<T>(T value, bool sync) where T : struct, IActionParam
		{
			if (m_AddTime.Value < value.Time.Value)
			{
				m_AddTime = value.Time;
			}
			if (sync)
			{
				Wait(value.Time);
			}
			m_List.Set(m_Index.ToString(), value);
			m_Index++;
		}

		public void Wait(Millisecond millisecond)
		{
			Time = Time.Add(millisecond);
			m_AddTime = Millisecond.FromSecond(0);
		}

		public ActionData CreateData()
		{
			return new ActionData
			{
				Total = Time.Add(m_AddTime),
				Data = m_List.Save()
			};
		}

	}
}