using System.Diagnostics;

namespace PG.Interface.Command.PasswordGeneration.Entities
{
	public class HumanReadableMessage(TraceLevel type, string format, params object[] args)
	{
		public TraceLevel Type { get; set; } = type;
		public string Format { get; set; } = format;
		public object[] Args { get; set; } = args;
	}
}