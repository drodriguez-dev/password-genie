using PG.Data.Files.Dictionaries;
using PG.Logic.Common;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loader;
using PG.Logic.Passwords.Loader.Entities;
using PG.Shared.Extensions;
using System.Text;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGenerator(DictionaryPasswordGeneratorOptions options, IDictionariesData dictionariesData) : PasswordGeneratorBase
	{
		private const int MINIMUM_AVERAGE_WORD_LENGTH = 4;
		private const int WORD_VARIANCE_DIVIDER = 2;
		private readonly DictionaryPasswordGeneratorOptions _options = options;
		private readonly WordDictionaryLoader _dictionary = new(dictionariesData);

		protected override bool IncludeSetSymbols => _options.IncludeSetSymbols;
		protected override bool IncludeMarkSymbols => _options.IncludeMarkSymbols;
		protected override bool IncludeSeparatorSymbols => _options.IncludeSeparatorSymbols;
		protected override char[] CustomSpecialChars => _options.CustomSpecialCharacters;
		protected override bool RemoveHighAsciiCharacters => _options.RemoveHighAsciiCharacters;

		public override string Generate()
		{
			_dictionary.Load(_options.File);

			StringBuilder passwords = new();
			foreach (var passwordPart in GeneratePasswordParts())
				passwords.AppendLine(passwordPart);

			return passwords.ToString();
		}

		private IEnumerable<string> GeneratePasswordParts()
		{
			if (_options.NumberOfPasswords < 1)
				throw new InvalidOptionException("At least one password must be requested");

			if (_options.NumberOfWords < 1)
				throw new InvalidOptionException("At least one word must be requested");

			if (_options.AverageWordLength < MINIMUM_AVERAGE_WORD_LENGTH)
				throw new ArgumentOutOfRangeException(nameof(_options.AverageWordLength), $"Average length must be at least {MINIMUM_AVERAGE_WORD_LENGTH}.");

			int totalCharacterCount = (_options.AverageWordLength * _options.NumberOfWords) + _options.NumberOfNumbers + _options.NumberOfSpecialCharacters;
			if (totalCharacterCount <= 0)
				throw new InvalidOptionException("At least one character group must be included.");

			if (_options.MinimumLength > totalCharacterCount)
				throw new InvalidOptionException($"Minimum length must be lower to the sum of the number of letters, numbers, and special characters ({totalCharacterCount}).");

			if (_options.DepthLevel > _options.AverageWordLength)
				throw new InvalidOptionException($"Depth level must be lower than the average word length ({_options.DepthLevel}).");

			return BuildPasswordParts(_options.NumberOfPasswords, _options.MinimumLength);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Too complex")]
		protected override string BuildPasswordPart()
		{
			Random random = GetRandomNumberGenerator();

			List<string> words = GenerateWords(_options.NumberOfWords, _options.AverageWordLength, _options.DepthLevel).ToList();
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

		/// <summary>
		/// Generates a random set of words based on the dictionary provided.
		/// </summary>
		internal IEnumerable<string> GenerateWords(int numberOfWords, int averageLength, int depthLevel)
		{
			foreach (int _ in Enumerable.Range(0, numberOfWords))
			{
				string? word;
				int iterations = 0;

				// Generate a word that is not already in the dictionary and that does not start with any of the words in the dictionary.
				do { word = GenerateWord(averageLength, depthLevel); }
				while (iterations++ <= Constants.MAX_ITERATIONS && _dictionary.IsLeafNodeReached(word));

				if (iterations > Constants.MAX_ITERATIONS)
					throw new InvalidOperationException("Could not generate a word that is not already in the dictionary with the current settings.");

				yield return word;
			}
		}

		/// <summary>
		/// Generates a word based on the dictionary provided. Word length is variable depending on the average word length, it's variance half of the 
		/// average length.
		/// </summary>
		/// <remarks>
		/// Uses a tree structure based on the dictionary to generate fictitious but language-like words.
		/// </remarks>
		private string GenerateWord(int averageLength, int depthLevel)
		{
			Random random = GetRandomNumberGenerator();

			var wordBuilder = new StringBuilder();

			// The variance is half of the average length. For example, if the average length is 8, the variance is 4; so the word length can be
			// between 4 and 12.
			var wordLengthVariance = Math.Max(1, averageLength / WORD_VARIANCE_DIVIDER);
			int wordLength = averageLength + (random.Next(wordLengthVariance * 2) - wordLengthVariance);

			ITreeNodeWithChildren<char> node = _dictionary.WordTree.Root;
			foreach (int _ in Enumerable.Range(0, wordLength))
			{
				var children = node.Children.Select(kvp => kvp.Value)
						.Where(tn => !RemoveHighAsciiCharacters || tn.Value < 128)
						.ToList();

				if (children.Count == 0) break;

				var next = children[random.Next(children.Count)];
				wordBuilder.Append(next.Value);

				node = next;
				if (wordBuilder.Length >= depthLevel && !_dictionary.TrySearchLastPossibleLeafNode(wordBuilder.ToString(), depthLevel, out node))
					break;
			}

			// Return the word with the first letter capitalized.
			string word = wordBuilder.ToString();
			return char.ToUpper(word[0]) + word[1..];
		}
	}
}
