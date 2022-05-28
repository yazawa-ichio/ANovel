using UnityEngine;

namespace ANovel.Core.Tests
{

	[TagName("test_log", Symbol = "TEST")]
	public class TestLogCommand : Command
	{
		[Argument]
		public string Message { get; private set; }

		protected override void Execute()
		{
			Debug.Log(Message);
		}
	}

	[TagName("test_macro_define", Symbol = "TEST")]
	public class TestMacroDefineCommand : Command
	{
		[Argument]
		public string Message { get; private set; }

		protected override void Execute()
		{
			Debug.Log(Message);
		}
	}

}