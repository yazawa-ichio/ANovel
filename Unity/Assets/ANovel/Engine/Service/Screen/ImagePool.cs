using System.Linq;
using UnityEngine;

namespace ANovel.Engine
{
	public class ImagePool : ComponentPool<ImageObject>
	{
		IEngineTime m_Time;
		int m_Layer;
		bool m_OrderDitry;

		public ImagePool(Transform root, IEngineTime time) : base(root)
		{
			m_Layer = root.gameObject.layer;
			m_Time = time;
		}

		protected override void OnCreate(ImageObject obj)
		{
			obj.gameObject.layer = m_Layer;
			obj.SetOwner(this);
		}

		public override ImageObject Get()
		{
			throw new System.InvalidOperationException("use Get(ILevel level)");
		}

		public ImageObject Get(ILevel level)
		{
			var ret = base.Get();
			ret.SetLevel(level);
			ret.SetTime(m_Time);
			SetOrderDitry();
			return ret;
		}

		public override void Return(ImageObject item)
		{
			item.OnReturn();
			base.Return(item);
		}

		public void SetOrderDitry()
		{
			m_OrderDitry = true;
		}

		public void TrySort()
		{
			if (!m_OrderDitry)
			{
				return;
			}
			m_OrderDitry = false;
			foreach (var active in Active.OrderBy(x => x.AutoOrder))
			{
				active.transform.SetAsLastSibling();
			}
		}

	}
}