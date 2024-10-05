namespace PG.Logic.Passwords.Generators.Entities
{
	public class CommonPasswordGeneratorOptions
	{
		public int NumberOfPasswords { get; set; }

		public int MinimumLength { get; set; }

		public int NumberOfNumbers { get; set; }

		public int NumberOfSpecialCharacters { get; set; }

		/// <summary>
		/// If true, the password will contain symbols like: (){}[]<>.
		/// </summary>
		public bool IncludeSetSymbols { get; set; }

		public bool IncludeSeparatorSymbols { get; set; }

		/// <summary>
		/// If true, the password will contain symbols like: !@#$%^&*.
		/// </summary>
		public bool IncludeMarkSymbols { get; set; }

		/// <summary>
		/// If true, the password will not contain characters from the high ASCII range.
		/// </summary>
		public bool RemoveHighAsciiCharacters { get; set; }

		public char[] CustomSpecialCharacters { get; set; } = [];

		public KeystrokeOrder KeystrokeOrder { get; set; }

		public override string ToString() => $"Q: {NumberOfPasswords}, min: {MinimumLength} (N:{NumberOfNumbers}, S:{NumberOfSpecialCharacters})";
	}
}
