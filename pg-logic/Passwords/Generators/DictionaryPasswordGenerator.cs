using PG.Entities.WordTrees;
using PG.Logic.Common;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Extensions;
using PG.Shared.Services;
using System.Text;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGenerator(DictionaryPasswordGeneratorOptions options, RandomService random, WordDictionaryTree wordTree) : PasswordGeneratorBase(random)
	{
		private const int MINIMUM_AVERAGE_WORD_LENGTH = 4;

		private DictionaryPasswordGeneratorOptions _options = options;

		protected override bool IncludeSetSymbols => _options.IncludeSetSymbols;
		protected override bool IncludeMarkSymbols => _options.IncludeMarkSymbols;
		protected override bool IncludeSeparatorSymbols => _options.IncludeSeparatorSymbols;
		protected override char[] CustomSpecialChars => _options.CustomSpecialCharacters;
		protected override bool RemoveHighAsciiCharacters => _options.RemoveHighAsciiCharacters;
		protected override KeystrokeOrder KeystrokeOrder => _options.KeystrokeOrder;

		private readonly WordDictionaryTree _wordTree = wordTree;

		public override void Configure(CommonPasswordGeneratorOptions config)
		{
			if (config is not DictionaryPasswordGeneratorOptions dictionaryOptions)
				throw new ArgumentException($"Invalid configuration type ({config.GetType()}).", nameof(config));

			_options = dictionaryOptions;
		}

		protected override IEnumerable<string> GeneratePasswordParts()
		{
			if (_options.NumberOfPasswords < 1)
				throw new InvalidOptionException("At least one password must be requested");

			if (_options.NumberOfWords < 1)
				throw new InvalidOptionException("At least one word must be requested");

			if (_options.AverageWordLength < MINIMUM_AVERAGE_WORD_LENGTH)
				throw new InvalidOptionException($"Average length must be at least {MINIMUM_AVERAGE_WORD_LENGTH}.");

			int totalCharacterCount = (_options.AverageWordLength * _options.NumberOfWords) + _options.NumberOfNumbers + _options.NumberOfSpecialCharacters;
			if (totalCharacterCount <= 0)
				throw new InvalidOptionException("At least one character group must be included.");

			// Calculate the maximum depth level based on the average word length. This ensures that the depth level
			// is less than half of the average word length and decreases as the average word length increases. This
			// validation avoids the "Max iterations reached" exception.
			int maxDepthLevel = (int)Math.Round(Math.Sqrt(_options.AverageWordLength) * Constants.DEPTH_LEVEL_COEFFICIENT, digits: 0);
			if (_options.DepthLevel > maxDepthLevel)
				throw new InvalidOptionException($"Depth level must be lower for the specified average length (max: {maxDepthLevel}).");

			return BuildPasswordParts(_options.NumberOfPasswords);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Too complex")]
		protected override string BuildPasswordPart()
		{
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
			string @return = string.Join(string.Empty, passwordElements);
			_random.CommitEntropy();

			return @return;

			static bool EndsWithWhitespace(string text) => text.Length > 0 && text[^1].IsWhitespace();

			// Returns a random position from the list of available positions and removes it from the list.
			int PopPosition(int min, int max)
			{
				if (positions.Count == 0) throw new InvalidOperationException("There are no more positions available.");

				// Ensure the min and max values are within the range of the positions list.
				min = Math.Max(1, Math.Min(positions.Count, min));
				max = Math.Max(1, Math.Min(positions.Count, max));

				int index = _random.Next(max - min + 1) + min - 1;
				int position = positions[index];
				positions.RemoveAt(index);

				return position;
			}
		}

		/// <summary>
		/// Generate a random string of characters.
		/// </summary>
		protected IEnumerable<string> GenerateNumbers(int length)
		{
			foreach (int _ in Enumerable.Range(0, length))
				yield return _numbers[_random.Next(_numbers.Length)].ToString();

			_random.CommitEntropy();
		}

		/// <summary>
		/// Generates a random set of symbols
		/// </summary>
		protected IEnumerable<string> GenerateSymbols(int length)
		{
			if (length <= 0) yield break;

			char[] symbols = [.. GetAvailableSymbols()];
			if (symbols.Length == 0)
				throw new InvalidOperationException("No symbols are available. Either provide custom symbols or enable the default ones.");

			foreach (int _ in Enumerable.Range(0, length))
				yield return symbols[_random.Next(symbols.Length)].ToString();

			_random.CommitEntropy();
		}

		/// <summary>
		/// Generates a random set of words based on the dictionary provided.
		/// </summary>
		internal IEnumerable<string> GenerateWords(int numberOfWords, int averageLength, int depthLevel)
		{
			HandSide currentHand = ChooseFirstHand();
			_random.CommitEntropy();

			var wordLengths = _random.GenerateNumbersForAverage(numberOfWords, averageLength);

			foreach (int wordLength in wordLengths)
			{
				string? word;
				int iterations = 0;

				// Generate a word that is not already in the dictionary (IsLeafNodeReached) and that does not start with any of the words in the dictionary.
				do
				{
					// If the previos word was not valid, discard the entropy and try again.
					_random.DiscardEntropy();
					word = GenerateWord(wordLength, depthLevel, ref currentHand);
				}
				while (iterations++ < Constants.MAX_ITERATIONS && string.IsNullOrEmpty(word) && IsLeafNodeReached(word));

				if (iterations >= Constants.MAX_ITERATIONS)
					throw new InvalidOperationException("Max iterations reached without being able to generate a valid word.");

				yield return word;

				_random.CommitEntropy();

				currentHand = ChooseHand(currentHand);
			}
		}

		/// <summary>
		/// Searches for a leaf node in the dictionary tree by traversing the tree using the specified word and returns true if the leaf node is reached; 
		/// the word is found.
		/// </summary>
		public bool IsLeafNodeReached(string word) => TrySearchLeafNode(word, out _);

		/// <summary>
		/// Searches for a leaf node in the dictionary tree by traversing the tree using the specified word. If the word is not found, the search stops at 
		/// the last node that was found and returns false.
		/// </summary>
		private bool TrySearchLeafNode(string word, out ITreeNode<string> node)
		{
			node = _wordTree?.Root
				?? throw new InvalidOperationException("The word tree has not been initialized.");

			foreach (var letter in word)
			{
				var children = node.Children.Select(kvp => kvp.Value)
					.FirstOrDefault(tn => tn.Value.ToString().Equals(letter.ToString(), StringComparison.InvariantCultureIgnoreCase));

				if (children == default) return false;

				node = children;
			}

			return true;
		}

		/// <summary>
		/// Searches for the last possible leaf node in the dictionary tree by successively removing the last character of the word. If there is no valid 
		/// node, the search stops at the last node that was found and returns false.
		/// </summary>
		public bool TrySearchLastPossibleLeafNode(string word, int depthLevel, out ITreeNode<string> node)
		{
			if (depthLevel <= 0)
				throw new ArgumentOutOfRangeException(nameof(depthLevel), "Depth level must be greater than zero.");

			bool found;
			do { found = TrySearchLeafNode(word.Right(depthLevel--), out node); }
			while (!found && depthLevel > 0);

			return found;
		}

		protected override HandSide ChooseHand(HandSide currentHand)
		{
			if (KeystrokeOrder == KeystrokeOrder.AlternatingWord)
				currentHand = currentHand == HandSide.Left ? HandSide.Right : HandSide.Left;
			return currentHand;
		}

		/// <summary>
		/// Generates a word based on the dictionary provided. Word length is variable depending on the average word length, it's variance half of the 
		/// average length.
		/// </summary>
		/// <remarks>
		/// Uses a tree structure based on the dictionary to generate fictitious but language-like words.
		/// </remarks>
		private string GenerateWord(int wordLength, int depthLevel, ref HandSide currentHand)
		{
			// TODO - 2025-04-06 - Refactor this method to use a more efficient algorithm.
			var wordBuilder = new StringBuilder();

			ITreeNode<string> startNode = _wordTree?.Root
				?? throw new InvalidOperationException("The word tree has not been initialized.");

			int iterations = 0;
			do
			{
				Finger? curFinger = null;
				ITreeNode<string> node = startNode;

				// If the previos word was not valid, discard the entropy and try again.
				_random.DiscardEntropy();
				wordBuilder.Clear();

				for (int index = 0; index < wordLength; index++)
				{
					HandSide curHand = currentHand;
					var children = node.Children.Select(kvp => kvp.Value)
							.Where(tn => !RemoveHighAsciiCharacters || tn.Value[0] < 128)
							.Where(tn => IsProperHand(tn.Value[0], curHand))
							.Where(tn => IsProperFinger(tn.Value[0], curHand, curFinger))
							.Where(tn => !IsInLastChars(tn.Value[0], wordBuilder.ToString(), depthLevel))
							.ToList();

					if (children.Count == 0) break;

					var next = children[_random.Next(children.Count)];
					wordBuilder = wordBuilder.Append(next.Value);

					curFinger = GetFingerForKeystroke(next.Value);

					if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
						currentHand = currentHand == HandSide.Left ? HandSide.Right : HandSide.Left;

					node = next;

					// If the word is already in the dictionary, break the loop.
					if (wordBuilder.Length >= depthLevel && !TrySearchLastPossibleLeafNode(wordBuilder.ToString(), depthLevel, out node))
						break;
				}
			} while (wordBuilder.Length < wordLength && iterations++ < Constants.MAX_ITERATIONS);

			if (iterations >= Constants.MAX_ITERATIONS)
				throw new InvalidOperationException("Max iterations reached without being able to generate a valid word. Try to reduce the restrictions.");

			// Return the word with the first letter capitalized.
			string word = wordBuilder.ToString();

			if (word.Length == 0) return string.Empty;
			else if (word.Length == 1) return word.ToUpper();
			else return char.ToUpper(word[0]) + word[1..];
		}
	}
}
