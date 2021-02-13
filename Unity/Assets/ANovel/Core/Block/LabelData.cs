using System.Collections.Generic;

namespace ANovel.Core
{

	public class LabelData
	{
		TagParam m_Param = new TagParam();

		public string Name { get; private set; }

		public int BlockCount { get; private set; }

		public void Set(in LineData data, List<IParamConverter> converters)
		{
			m_Param.Set(in data, converters);
			Name = m_Param.Name;
			BlockCount = 0;
		}

		public void Reset()
		{
			Name = null;
			BlockCount = 0;
		}

		public BlockLabelInfo GetInfo(in LineData data)
		{
			return new BlockLabelInfo(data.FileName, Name, BlockCount++, data.Index);
		}

	}

}