using System.Collections.Generic;

namespace ANovel.Core
{

	public class EnvDataHook
	{
		List<IEnvDataCustomProcessor> m_List = new List<IEnvDataCustomProcessor>();

		public void Add(IEnvDataCustomProcessor processor)
		{
			m_List.Add(processor);
			m_List.Sort((x, y) => y.Priority - x.Priority);
		}

		public void Remove(IEnvDataCustomProcessor processor)
		{
			m_List.Remove(processor);
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
						cmd.SetMetaData(block.Meta);
						cmd.UpdateEnvData(data);
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

	}

}