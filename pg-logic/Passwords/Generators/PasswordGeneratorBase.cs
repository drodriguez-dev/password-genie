using PG.Logic.Common;
using PG.Shared.Services;

namespace PG.Logic.Passwords.Generators
{
	public abstract class PasswordGeneratorBase(RandomService random) : IPasswordGenerator
	{
		protected readonly RandomService _random = random;
		private readonly List<double> _entropyValues = [];

		protected abstract bool IncludeSetSymbols { get; }
		protected abstract bool IncludeMarkSymbols { get; }
		protected abstract bool IncludeSeparatorSymbols { get; }
		protected abstract char[] CustomSpecialChars { get; }
		protected abstract bool RemoveHighAsciiCharacters { get; }

		protected static readonly char[] _setSymbols = @"()[]{}<>".ToCharArray();
		protected static readonly char[] _markSymbols = @"!@#$%^*+=|;:\""?".ToCharArray();
		protected static readonly char[] _separatorSymbols = @" -_/\&,.".ToCharArray();

		public abstract string Generate();

		protected abstract string BuildPasswordPart();

		protected virtual IEnumerable<string> BuildPasswordParts(int numberOfPasswords, int minimumLength)
		{
			foreach (int _ in Enumerable.Range(0, numberOfPasswords))
			{
				string passwordPart;
				int iterations = 0;
				_random.ResetEntropy();
				do
				{
					// If the previous password part was not valid, discard the entropy and try again.
					_random.DiscardEntropy();
					passwordPart = BuildPasswordPart();
				}
				while (iterations++ < Constants.MAX_ITERATIONS && passwordPart.Length < minimumLength);

				if (iterations >= Constants.MAX_ITERATIONS)
					throw new InvalidOperationException("Could not generate a password with the required length and the current settings.");

				yield return passwordPart;

				_random.CommitEntropy();
				_entropyValues.Add(_random.GetBitsOfEntropy());
			}
		}

		/// <summary>
		/// Generate a random string of characters.
		/// </summary>
		protected IEnumerable<string> GenerateNumbers(int length)
		{
			foreach (int _ in Enumerable.Range(0, length))
				yield return _random.Next(0, 10).ToString();

			_random.CommitEntropy();
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

			foreach (int _ in Enumerable.Range(0, length))
				yield return symbols[_random.Next(symbols.Length)].ToString();

			_random.CommitEntropy();
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

		public double GetAndResetPasswordEntropy()
		{
			double entropy = _entropyValues.Sum() / _entropyValues.Count;
			_entropyValues.Clear();

			return entropy;
		}
	}
}
