using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loader;
using PG.Logic.Passwords.Loader.Entities;
using System.Text;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGenerator(DictionaryPasswordGeneratorOptions options) : PasswordGeneratorBase
	{
		private readonly DictionaryPasswordGeneratorOptions _options = options;
		protected override bool IncludeSetSymbols => _options.IncludeSetSymbols;
		protected override bool IncludeMarkSymbols => _options.IncludeMarkSymbols;
		protected override bool IncludeSeparatorSymbols => _options.IncludeSeparatorSymbols;
		protected override bool RemoveHighAsciiCharacters => _options.RemoveHighAsciiCharacters;

		public required IDictionariesData DictionariesData { get; set; }

		public override string Generate()
		{
			var dictionary = new DictionaryLoader(DictionariesData).Load(_options.File.FullName);

			StringBuilder passwords = new();
			foreach (var passwordPart in GeneratePasswordParts(_options, dictionary))
				passwords.AppendLine(passwordPart);

			// Convert the StringBuilder to a string and remove the last line break.
			return passwords.ToString().Remove(passwords.Length - Environment.NewLine.Length, Environment.NewLine.Length);
		}

		private IEnumerable<string> GeneratePasswordParts(DictionaryPasswordGeneratorOptions options, WordDictionary dictionary)
		{
			if (options.NumberOfPasswords < 0)
				throw new ArgumentOutOfRangeException(nameof(options), "Number of passwords must be greater than or equal to 0.");

			int totalCharacterCount = (options.AverageWordLength * options.NumberOfWords) + options.NumberOfNumbers + options.NumberOfSpecialCharacters;
			if (totalCharacterCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(options), "At least one character group must be included.");

			if (options.MinimumLength > totalCharacterCount)
				throw new ArgumentOutOfRangeException(nameof(options), $"Minimum length must be lower to the sum of the number of letters, numbers, and special characters ({totalCharacterCount}).");

			return BuildPasswordParts(options, dictionary);
		}

		private IEnumerable<string> BuildPasswordParts(DictionaryPasswordGeneratorOptions options, WordDictionary dictionary)
		{
			foreach (int _ in Enumerable.Range(0, options.NumberOfPasswords))
			{
				string passwordPart;
				do { passwordPart = BuildPasswordPart(options, dictionary); }
				while (passwordPart.Length < options.MinimumLength);

				yield return passwordPart;
			}
		}

		private string BuildPasswordPart(DictionaryPasswordGeneratorOptions options, WordDictionary dictionary)
		{
			Random random = GetRandomNumberGenerator();

			List<string> words = GenerateWords(dictionary, options.NumberOfWords, options.AverageWordLength).ToList();
			IEnumerable<string> numbers = GenerateNumbers(options.NumberOfNumbers);
			IEnumerable<string> symbols = GenerateSymbols(options.NumberOfSpecialCharacters);

			var w = ((IEnumerable<string>)words).Concat(numbers).Concat(symbols);

			//// The passwords always start with the first word, then: the rest of the words, numbers, and symbols in a random order.
			var strings = new string[] { words[0] }
				.Concat(words[1..]
					.Concat(numbers).Concat(symbols)
					.Where(s => s.Length > 0)
					.OrderBy(_ => random.Next())
				);

			return string.Join(string.Empty, strings);
		}
	}
}
