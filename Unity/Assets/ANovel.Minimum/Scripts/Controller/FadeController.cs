using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Minimum
{
	public class FadeConfig
	{
		public float Time = 0.3f;
		public Color Color = new Color(0, 0, 0, 1);
	}

	public interface IFadeController : IController
	{
		CommandCoroutine FadeIn(FadeConfig config);
		CommandCoroutine FadeOut(float time);
	}

	public class FadeController : ControllerBase, IFadeController
	{
		[SerializeField]
		Image m_Image = default;
		[SerializeField]
		CanvasGroup m_Group = default;

		public override Type ControllerType => typeof(IFadeController);

		void Awake()
		{
			m_Group.alpha = 0;
		}

		public CommandCoroutine FadeIn(FadeConfig config)
		{
			return new CommandCoroutine(this, FadeInImpl(config), FadeInComplete);
		}

		public CommandCoroutine FadeOut(float time)
		{
			return new CommandCoroutine(this, FadeOutImpl(time), FadeOutComplete);
		}

		IEnumerator FadeOutImpl(float fadeTime)
		{
			float time = 0;
			float alpha = m_Group.alpha;
			while (time < fadeTime)
			{
				time += DeltaTime;
				m_Group.alpha = (1f - time / fadeTime) * alpha;
				yield return null;
			}
		}

		void FadeOutComplete()
		{
			m_Group.alpha = 0f;
		}

		IEnumerator FadeInImpl(FadeConfig config)
		{
			float time = 0;
			float start = m_Group.alpha;
			float alpha = 1 - m_Group.alpha;
			m_Image.color = config.Color;
			while (time < config.Time)
			{
				time += DeltaTime;
				m_Group.alpha = start + (time / config.Time) * alpha;
				yield return null;
			}
		}

		void FadeInComplete()
		{
			m_Group.alpha = 1f;
		}

	}
}