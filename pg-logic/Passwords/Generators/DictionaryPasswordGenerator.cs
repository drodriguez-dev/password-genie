using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loader;
using PG.Logic.Passwords.Loader.Entities;
using PG.Shared.Extensions;
using System.Text;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGenerator(DictionaryPasswordGeneratorOptions options) : PasswordGeneratorBase
	{
		private readonly DictionaryPasswordGeneratorOptions _options = options;
		protected override bool IncludeSetSymbols => _options.IncludeSetSymbols;
		protected override bool IncludeMarkSymbols => _options.IncludeMarkSymbols;
		protected override bool IncludeSeparatorSymbols => _options.IncludeSeparatorSymbols;
		protected override char[] CustomSpecialChars => _options.CustomSpecialCharacters;
		protected override bool RemoveHighAsciiCharacters => _options.RemoveHighAsciiCharacters;

		public required IDictionariesData DictionariesData { get; set; }

		public override string Generate()
		{
			var dictionary = new DictionaryLoader(DictionariesData).Load(_options.File);

			StringBuilder passwords = new();
			foreach (var passwordPart in GeneratePasswordParts(dictionary))
				passwords.AppendLine(passwordPart);

			// Convert the StringBuilder to a string and remove the last line break.
			return passwords.ToString().Remove(passwords.Length - Environment.NewLine.Length, Environment.NewLine.Length);
		}

		private IEnumerable<string> GeneratePasswordParts(WordDictionary dictionary)
		{
			if (_options.NumberOfPasswords < 1)
				throw new InvalidOptionException("At least one password must be requested");

			if (_options.NumberOfWords < 1)
				throw new InvalidOptionException("At least one word must be requested");

			int totalCharacterCount = (_options.AverageWordLength * _options.NumberOfWords) + _options.NumberOfNumbers + _options.NumberOfSpecialCharacters;
			if (totalCharacterCount <= 0)
				throw new InvalidOptionException("At least one character group must be included.");

			if (_options.MinimumLength > totalCharacterCount)
				throw new InvalidOptionException($"Minimum length must be lower to the sum of the number of letters, numbers, and special characters ({totalCharacterCount}).");

			return BuildPasswordParts(dictionary);
		}

		private IEnumerable<string> BuildPasswordParts(WordDictionary dictionary)
		{
			foreach (int _ in Enumerable.Range(0, _options.NumberOfPasswords))
			{
				string passwordPart;
				do { passwordPart = BuildPasswordPart(dictionary); }
				while (passwordPart.Length < _options.MinimumLength);

				yield return passwordPart;
			}
		}

		private string BuildPasswordPart(WordDictionary dictionary)
		{
			Random random = GetRandomNumberGenerator();

			List<string> words = GenerateWords(dictionary, _options.NumberOfWords, _options.AverageWordLength).ToList();
			List<string> numbers = GenerateNumbers(_options.NumberOfNumbers).ToList();
			List<string> symbols = GenerateSymbols(_options.NumberOfSpecialCharacters)
				.OrderBy(s => EndsWithWhitespace(s) ? int.MinValue : int.MaxValue).ToList();

			// List of positions that will be used to determine the order of the password items. It's initialized with all the possible positions ordered from 0 to n - 1.
			List<int> positions = Enumerable.Range(0, Math.Max(0, words.Count - 1) + numbers.Count + symbols.Count).ToList();

			// The password always starts with the first word, then: the rest of the words, numbers, and symbols in a random order.
			// Not printable characters will never be the last character in the password.
			IEnumerable<string> passwordElements = words.Skip(1)
				.Concat(numbers).Concat(symbols)
				.OrderBy(s => EndsWithWhitespace(s) ? PopPosition(1, positions.Count) : PopPosition(2, positions.Count - 1));

			passwordElements = words.Take(1).Concat(passwordElements);

			return string.Join(string.Empty, passwordElements);

			static bool EndsWithWhitespace(string text) => text.Length > 0 && text[^1].IsWhitespace();

			// Returns a random position from the list of available positions and removes it from the list.
			int PopPosition(int min, int max)
			{
				if (positions.Count == 0) throw new InvalidOperationException("There are no more positions available.");

				// Ensure the min and max values are within the range of the positions list.
				min = Math.Max(1, Math.Min(positions.Count, min));
				max = Math.Max(1, Math.Min(positions.Count, max));

				int index = random.Next(max - min + 1) + min - 1;
				int position = positions[index];
				positions.RemoveAt(index);

				return position;
			}
		}
	}
}
