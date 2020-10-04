using ANovel.Core;

namespace ANovel
{
	public class CommandNameAttribute : TagNameAttribute
	{
		public CommandNameAttribute(string name) : base(name, LineType.Command)
		{
		}
	}

	public class CommandFieldAttribute : TagFieldAttribute
	{
		public CommandFieldAttribute() : base() { }
	}
}