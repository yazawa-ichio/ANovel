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

	public class SystemCommandNameAttribute : TagNameAttribute
	{
		public SystemCommandNameAttribute(string name) : base(name, LineType.SystemCommand)
		{
		}
	}

	public class PreProcessNameAttribute : TagNameAttribute
	{
		public PreProcessNameAttribute(string name) : base(name, LineType.PreProcess)
		{
		}
	}
}