using System.Collections.Generic;

namespace ANovel.Core
{

	public class EnvDataHook
	{
		ServiceContainer m_Container;
		List<IEnvDataCustomProcessor> m_List = new List<IEnvDataCustomProcessor>();
		List<IPlayingEnvDataProcessor> m_PlayingList = new List<IPlayingEnvDataProcessor>();
		EnvData m_PlayingData = new EnvData();

		public EnvDataHook(ServiceContainer container)
		{
			m_Container = container;
		}

		public void Add(IEnvDataCustomProcessor processor)
		{
			m_List.Add(processor);
			m_List.Sort((x, y) => y.Priority - x.Priority);
		}

		public void Remove(IEnvDataCustomProcessor processor)
		{
			m_List.Remove(processor);
		}

		public void Add(IPlayingEnvDataProcessor processor)
		{
			m_PlayingList.Add(processor);
		}

		public void Remove(IPlayingEnvDataProcessor processor)
		{
			m_PlayingList.Remove(processor);
		}

		public void PreUpdate(IEnvData data, Block block)
		{
			foreach (var processor in m_List)
			{
				using (ListPool<ICommand>.Use(out var list))
				{
					processor.PreUpdate(new EnvDataUpdateParam(data, block, list));
					block.Commands.InsertRange(0, list);
				}
			}
		}

		public void PostUpdate(IEnvData data, Block block)
		{
			foreach (var processor in m_List)
			{
				using (ListPool<ICommand>.Use(out var list))
				{
					processor.PostUpdate(new EnvDataUpdateParam(data, block, list));
					block.Commands.AddRange(list);
					foreach (var cmd in list)
					{
						cmd.Init(m_Container, block.Meta, data);
					}
				}
			}
		}

		public void PostJump(IMetaData meta, IEnvData data)
		{
			foreach (var processor in m_List)
			{
				processor.PostJump(meta, data);
			}
		}

		public EnvDataSnapshot SavePlayingEnvData()
		{
			m_PlayingData.Clear();
			foreach (var processor in m_PlayingList)
			{
				processor.Store(m_PlayingData);
			}
			return m_PlayingData.Save();
		}

		public void LoadPlayingEnvData(EnvDataSnapshot playingSave)
		{
			m_PlayingData.Load(playingSave);
			foreach (var processor in m_PlayingList)
			{
				processor.Restore(m_PlayingData);
			}
		}
	}

}