using UnityEngine;
using UnityEngine.UI;

namespace ANovel.Service
{

	public class TextPrinter : TextPrinterBase
	{
		[SerializeField]
		Text m_NameText;
		[SerializeField]
		string m_NameFormat;
		[SerializeField]
		Text m_MessageText;

		protected override void AddMessage(string text)
		{
			m_MessageText.text += text;
		}

		protected override void SetMessage(string text)
		{
			m_MessageText.text = text;
		}

		protected override void SetName(string name)
		{
			if (string.IsNullOrEmpty(m_NameFormat) || string.IsNullOrEmpty(name))
			{
				m_NameText.text = name;
			}
			else
			{
				m_NameText.text = string.Format(m_NameFormat, name);
			}
		}
	}

}