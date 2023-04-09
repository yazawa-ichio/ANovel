using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ANovel.Engine.PostEffects
{
	public class PostEffectService : Service
	{
		public override Type ServiceType => typeof(PostEffectService);

		[SerializeReference]
		List<IPostEffect> m_PostEffects = new List<IPostEffect>()
		{
			new Bloom(),
			new Blur(),
			new SimpleColorEffect(),
			new HsvShift(),
		};

		IScreenService Screen => Container.Get<IScreenService>();

		List<PostEffectTask> m_Tasks = new List<PostEffectTask>();

		protected override void Initialize()
		{
			Screen.OnDeleteChild += OnDeleteChild;
			Screen.OnBeginSwap += OnBeginSwap;
		}

		public void Clear()
		{
			foreach (var task in m_Tasks)
			{
				Screen.PostEffects.Remove(task);
			}
			m_Tasks.Clear();
		}

		private void OnValidate()
		{
			foreach (var effects in m_PostEffects)
			{
				effects?.OnValidate();
			}
		}

		void OnBeginSwap(BeginSwapEvent e)
		{
			if (e.Copy)
			{
				foreach (var task in m_Tasks)
				{
					Screen.PostEffects.Add(task);
				}
			}
		}

		void OnDeleteChild(DeleteChildEvent e)
		{
			m_Tasks.Clear();
		}

		public void Run<T>(T value)
		{
			foreach (var effect in m_PostEffects)
			{
				if (!effect.IsParam(value))
				{
					continue;
				}
				var task = new PostEffectTask
				{
					Effect = effect,
					Param = value,
				};
				m_Tasks.Add(task);
				Screen.PostEffects.Add(task);
			}
		}

		protected override Task PostRestoreAsync(IMetaData meta, IEnvDataHolder data)
		{
			foreach (var kvp in data.GetAllByInterface<IPostEffectParam>())
			{
				var value = kvp.Value;
				foreach (var effect in m_PostEffects)
				{
					if (!effect.IsParam(value))
					{
						continue;
					}
					var task = new PostEffectTask
					{
						Effect = effect,
						Param = value,
					};
					m_Tasks.Add(task);
					Screen.PostEffects.Add(task);
				}
			}
			return Task.CompletedTask;
		}

	}

}