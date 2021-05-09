using UnityEngine;

namespace ANovel.Service
{
	public class ImagePool : ComponentPool<ImageObject>
	{
		IEngineTime m_Time;
		int m_Layer;

		public ImagePool(Transform root, IEngineTime time) : base(root)
		{
			m_Layer = root.gameObject.layer;
			m_Time = time;
		}

		protected override void OnCreate(ImageObject obj)
		{
			obj.gameObject.layer = m_Layer;
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
			return ret;
		}

		public override void Return(ImageObject item)
		{
			item.OnReturn();
			base.Return(item);
		}

	}
}