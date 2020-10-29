using System;
using System.Collections;
using UnityEngine;

namespace ANovel.Minimum
{
	public class CommandCoroutine
	{
		public static readonly CommandCoroutine Empty = new CommandCoroutine
		{
			IsEnd = true,
		};

		public bool IsRunning { get; private set; }

		public bool IsEnd { get; private set; }

		MonoBehaviour m_Behaviour;
		IEnumerator m_Enumerator;
		Action m_Finish;
		Coroutine m_Coroutine;

		private CommandCoroutine() { }

		public CommandCoroutine(MonoBehaviour behaviour, IEnumerator enumerator, Action finish)
		{
			m_Behaviour = behaviour;
			m_Enumerator = enumerator;
			m_Finish = finish;

		}

		public CommandCoroutine Run()
		{
			if (IsEnd) return this;
			m_Coroutine = m_Behaviour.StartCoroutine(RunImpl());
			return this;
		}

		IEnumerator RunImpl()
		{
			IsRunning = true;
			while (m_Enumerator.MoveNext())
			{
				yield return m_Enumerator.Current;
			}
			IsRunning = false;
			Finish();
		}

		public void Finish()
		{
			if (IsEnd) return;

			if (m_Coroutine != null)
			{
				m_Behaviour.StopCoroutine(m_Coroutine);
			}
			m_Coroutine = null;
			m_Behaviour = null;
			IsEnd = true;
			m_Finish?.Invoke();
			m_Finish = null;
		}

	}
}