using PG.Logic.Passwords.Loader.Entities;
using System.Text;

namespace PG.Logic.Passwords.Generators
{
	public abstract class PasswordGeneratorBase : IPasswordGenerator
	{
		private static int _seed = Environment.TickCount;
		protected static readonly ThreadLocal<Random> _random = new(() => new Random(Interlocked.Increment(ref _seed)));

		protected static readonly char[] _letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
		protected static readonly char[] _setSymbols = @"()[]{}<>".ToCharArray();
		protected static readonly char[] _markSymbols = @"!@#$%^*+=|;:\""?".ToCharArray();
		protected static readonly char[] _separatorSymbols = @" -_/\&,.".ToCharArray();

		protected abstract bool IncludeSetSymbols { get; }
		protected abstract bool IncludeMarkSymbols { get; }
		protected abstract bool IncludeSeparatorSymbols { get; }
		protected abstract char[] CustomSpecialChars { get; }
		protected abstract bool RemoveHighAsciiCharacters { get; }

		public abstract string Generate();

		protected static Random GetRandomNumberGenerator()
		{
			if (_random.Value == null)
				throw new InvalidOperationException("Random number generator is not initialized.");

			return _random.Value;
		}

		/// <summary>
		/// Generate a random string of characters.
		/// </summary>
		protected static IEnumerable<string> GenerateNumbers(int length)
		{
			Random random = GetRandomNumberGenerator();

			foreach (int _ in Enumerable.Range(0, length))
				yield return random.Next(0, 10).ToString();
		}

		/// <summary>
		/// Generates a random set of letters
		/// </summary>
		protected static IEnumerable<char> GenerateLetters(int length)
		{
			Random random = GetRandomNumberGenerator();

			foreach (int _ in Enumerable.Range(0, length))
				yield return _letters[random.Next(0, _letters.Length)];
		}

		/// <summary>
		/// Generates a random set of words based on the dictionary provided.
		/// </summary>
		internal IEnumerable<string> GenerateWords(WordDictionary dictionary, int numberofWords, int averageLength)
		{
			foreach (int _ in Enumerable.Range(0, numberofWords))
			{
				string? word;

				do { word = GenerateWord(dictionary.Root, averageLength); }
				while (dictionary.Words.Contains(word, StringComparer.InvariantCultureIgnoreCase));

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
		private string GenerateWord(ITreeNodeWithChildren<char> root, int averageLength)
		{
			if (averageLength < 2)
				throw new ArgumentOutOfRangeException(nameof(averageLength), "Average length must be at least 2.");

			Random random = GetRandomNumberGenerator();

			var wordBuilder = new StringBuilder();

			// The variance is half of the average length. For example, if the average length is 8, the variance is 4; so the word length can be
			// between 4 and 12.
			var wordLengthVariance = Math.Max(1, averageLength / 2);

			var node = root;
			foreach (int _ in Enumerable.Range(0, averageLength + (random.Next(wordLengthVariance * 2) - wordLengthVariance)))
			{
				var children = node.Children.Select(n => n.Value).ToList();
				if (RemoveHighAsciiCharacters)
					children = children.Where(c => c.Value < 128).ToList();

				if (children.Count == 0) break;

				var next = children[random.Next(children.Count)];
				wordBuilder.Append(next.Value);

				node = next;
			}

			// Return the word with the first letter capitalized.
			string word = wordBuilder.ToString();
			return char.ToUpper(word[0]) + word[1..];
		}

		/// <summary>
		/// Generates a random set of symbols
		/// </summary>
		internal IEnumerable<string> GenerateSymbols(int length)
		{
			if (length <= 0) yield break;

			char[] symbols = GetAvailableSymbols().ToArray();
			if (symbols.Length == 0)
				throw new InvalidOperationException("No symbols are available. Either provide custom symbols or enable the default ones.");

			Random random = GetRandomNumberGenerator();

			foreach (int _ in Enumerable.Range(0, length))
				yield return symbols[random.Next(symbols.Length)].ToString();
		}

		/// <summary>
		/// Chooses between the custom or the default set and returns the available symbols.
		/// </summary>
		private IEnumerable<char> GetAvailableSymbols()
		{
			IEnumerable<char> symbols = CustomSpecialChars;
			if (!symbols.Any())
			{
				if (IncludeSetSymbols)
					symbols = symbols.Concat(_setSymbols);

				if (IncludeMarkSymbols)
					symbols = symbols.Concat(_markSymbols);

				if (IncludeSeparatorSymbols)
					symbols = symbols.Concat(_separatorSymbols);
			}

			return symbols;
		}
	}
}
