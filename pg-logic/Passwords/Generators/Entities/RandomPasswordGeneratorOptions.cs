namespace PG.Logic.Passwords.Generators.Entities
{
	public class RandomPasswordGeneratorOptions : CommonPasswordGeneratorOptions
	{
		public int NumberOfLetters { get; set; }

		public override string ToString() => $"Q: {NumberOfPasswords} (L: {NumberOfLetters}, N:{NumberOfNumbers}, S:{NumberOfSpecialCharacters})";
	}
}
