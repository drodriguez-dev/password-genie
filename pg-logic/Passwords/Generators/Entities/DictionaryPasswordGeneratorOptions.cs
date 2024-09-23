namespace PG.Logic.Passwords.Generators.Entities
{
	public class DictionaryPasswordGeneratorOptions : CommonPasswordGeneratorOptions
	{
		public required string File { get; set; }
		public int NumberOfWords { get; set; }
		public int AverageWordLength { get; set; }

		public override string ToString() => $"Q: {NumberOfPasswords}, min: {MinimumLength} (W: {NumberOfWords}, N:{NumberOfNumbers}, S:{NumberOfSpecialCharacters})";
	}
}
