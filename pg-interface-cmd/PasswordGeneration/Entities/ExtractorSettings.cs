namespace PG.Interface.Command.PasswordGeneration.Entities
{
	internal class ExtractorSettings : CommonSettings
	{
		public FileInfo? Input { get; set; }
		public FileInfo? Output { get; set; }
		public bool Overwrite { get; set; } = false;
	}
}
