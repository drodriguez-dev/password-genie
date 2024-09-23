using PG.Logic.Passwords.Loader.Entities;
using System.Text;

namespace PG.Logic.Passwords.Generators
{
	public abstract class PasswordGeneratorBase : IPasswordGenerator
	{
		private static int _seed = Environment.TickCount;
		protected static readonly ThreadLocal<Random> _random = new(() => new Random(Interlocked.Increment(ref _seed)));

		protected static readonly char[] _letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

		protected abstract bool IncludeSetSymbols { get; }
		protected abstract bool IncludeMarkSymbols { get; }
		protected abstract bool IncludeSeparatorSymbols { get; }
		protected abstract bool RemoveHighAsciiCharacters { get; }

		public abstract string Generate();

		/// <summary>
		/// Generate a random string of characters.
		/// </summary>
		protected static IEnumerable<string> GenerateNumbers(int length)
		{
			if (_random.Value == null)
				throw new InvalidOperationException("Random number generator is not initialized.");

			foreach (int _ in Enumerable.Range(0, length))
				yield return _random.Value.Next(0, 10).ToString();
		}

		/// <summary>
		/// Generates a random set of letters
		/// </summary>
		protected static IEnumerable<char> GenerateLetters(int length)
		{
			if (_random.Value == null)
				throw new InvalidOperationException("Random number generator is not initialized.");

			foreach (int _ in Enumerable.Range(0, length))
				yield return _letters[_random.Value.Next(0, _letters.Length)];
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
			if (_random.Value == null)
				throw new InvalidOperationException("Random number generator is not initialized.");
			if (averageLength < 2)
				throw new ArgumentOutOfRangeException(nameof(averageLength), "Average length must be at least 2.");

			var wordBuilder = new StringBuilder();

			// The variance is half of the average length. For example, if the average length is 8, the variance is 4; so the word length can be
			// between 4 and 12.
			var wordLengthVariance = averageLength / 2;

			var node = root;
			foreach (int _ in Enumerable.Range(0, averageLength + _random.Value.Next(-wordLengthVariance, wordLengthVariance)))
			{
				var children = node.Children.Select(n => n.Value).ToList();
				if (RemoveHighAsciiCharacters)
					children = children.Where(c => c.Value < 128).ToList();

				if (children.Count == 0) break;

				var next = children[_random.Value.Next(children.Count)];
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
			if (_random.Value == null)
				throw new InvalidOperationException("Random number generator is not initialized.");

			List<char> symbols = [];
			if (IncludeSetSymbols)
				symbols.AddRange(@"()[]{}<>".ToCharArray());

			if (IncludeMarkSymbols)
				symbols.AddRange(@"!@#$%^*+=|;:\""?".ToCharArray());

			if (IncludeSeparatorSymbols)
				symbols.AddRange(@" -_/\&,.".ToCharArray());

			if (symbols.Count == 0)
				throw new InvalidOperationException("No symbols are selected to generate.");

			foreach (int _ in Enumerable.Range(0, length))
				yield return symbols[_random.Value.Next(symbols.Count)].ToString();
		}
	}
}
