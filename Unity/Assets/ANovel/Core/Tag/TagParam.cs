using System;
using System.Collections.Generic;

namespace ANovel.Core
{

	public class TagParam : Dictionary<string, string>
	{
		public LineData Data { get; private set; }

		public string Name
		{
			get => m_Name;
			set
			{
				if (!m_UnlockName) throw new InvalidOperationException("TagParam.Name set TagParam.Set scope allowed only");
				m_Name = value;
			}
		}

		string m_Name;
		bool m_UnlockName;

		public void Set(in LineData data) => Set(in data, null);

		public void Set(in LineData data, List<IParamConverter> converters)
		{

			Clear();
			Data = data;
			m_Name = data.ReadName(out var nameEndIndex);
			if (nameEndIndex > 0)
			{
				data.ReadKeyValue(nameEndIndex, this);
			}
			if (converters != null)
			{
				m_UnlockName = true;
				try
				{
					foreach (var converter in converters)
					{
						converter.Convert(this);
					}
				}
				finally
				{
					m_UnlockName = false;
				}
			}
		}

	}

}