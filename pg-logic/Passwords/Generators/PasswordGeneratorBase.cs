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

		protected abstract string BuildPasswordPart();

		protected static Random GetRandomNumberGenerator()
		{
			if (_random.Value == null)
				throw new InvalidOperationException("Random number generator is not initialized.");

			return _random.Value;
		}

		protected virtual IEnumerable<string> BuildPasswordParts(int numberOfPasswords, int minimumLength)
		{
			foreach (int _ in Enumerable.Range(0, numberOfPasswords))
			{
				string passwordPart;
				do { passwordPart = BuildPasswordPart(); }
				while (passwordPart.Length < minimumLength);

				yield return passwordPart;
			}
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
