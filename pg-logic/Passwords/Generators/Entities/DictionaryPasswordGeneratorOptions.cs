namespace PG.Logic.Passwords.Generators.Entities
{
	public class DictionaryPasswordGeneratorOptions : CommonPasswordGeneratorOptions
	{
		public required FileInfo File { get; set; }
		public int NumberOfWords { get; set; }
		public int AverageWordLength { get; set; }
	}
}
