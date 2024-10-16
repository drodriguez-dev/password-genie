using PG.Logic.Passwords.Generators.Entities;

namespace PG.Interface.Command.PasswordGeneration.Entities
{
	internal class PassGenieSettings
	{
		// Common options
		public int NumberOfPasswords { get; set; } = 1;
		public int Length { get; set; } = 12;
		public int NumberOfNumbers { get; set; } = 1;
		public int NumberOfSpecialCharacters { get; set; } = 1;
		public bool IncludeGroupSymbols { get; set; } = true;
		public bool IncludeMarkSymbols { get; set; } = true;
		public bool IncludeSeparatorSymbols { get; set; } = true;
		public string CustomSymbols { get; set; } = string.Empty;
		public bool RemoveHighAsciiTable { get; set; } = false;
		public bool Verbose { get; set; }

		// Options for the random strategy
		public int NumberOfLetters { get; set; } = 10;

		// Options for the dictionary strategy
		public FileInfo? Dictionary { get; set; }
		public FileInfo? WordTree { get; set; }
		public int NumberOfWords { get; set; } = 2;
		public int AverageWordLength { get; set; } = 6;
		public int DepthLevel { get; set; } = 3;
		public KeystrokeOrder KeystrokeOrder { get; set; } = KeystrokeOrder.Random;
	}
}
