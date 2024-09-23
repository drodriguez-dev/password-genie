using PG.Logic.Passwords.Generators.Entities;
using System.Text;

namespace PG.Logic.Passwords.Generators
{
	public class RandomPasswordGenerator(RandomPasswordGeneratorOptions options) : PasswordGeneratorBase
	{
		private readonly RandomPasswordGeneratorOptions _options = options;
		protected override bool IncludeSetSymbols => _options.IncludeSetSymbols;
		protected override bool IncludeMarkSymbols => _options.IncludeMarkSymbols;
		protected override bool IncludeSeparatorSymbols => _options.IncludeSeparatorSymbols;
		protected override bool RemoveHighAsciiCharacters => _options.RemoveHighAsciiCharacters;

		public override string Generate()
		{
			StringBuilder password = new();
			foreach (var passwordPart in GeneratePasswordParts(_options))
				password.AppendLine(passwordPart);

			// Convert the StringBuilder to a string and remove the last line break
			return password.ToString().Remove(password.Length - Environment.NewLine.Length, Environment.NewLine.Length);
		}

		private IEnumerable<string> GeneratePasswordParts(RandomPasswordGeneratorOptions options)
		{
			if (options.NumberOfPasswords < 0)
				throw new ArgumentOutOfRangeException(nameof(options), "Number of passwords must be greater than or equal to 0.");

			int totalCharacterCount = options.NumberOfLetters + options.NumberOfNumbers + options.NumberOfSpecialCharacters;
			if (totalCharacterCount < 1)
				throw new ArgumentOutOfRangeException(nameof(options), "At least one character group must be included.");

			if (options.MinimumLength > totalCharacterCount)
				throw new ArgumentOutOfRangeException(nameof(options), $"Minimum length must be lower to the sum of the number of letters, numbers, and special characters ({totalCharacterCount}).");

			return GenerateSimplePasswordParts(options);
		}

		private IEnumerable<string> GenerateSimplePasswordParts(RandomPasswordGeneratorOptions options)
		{
			foreach (int _ in Enumerable.Range(0, options.NumberOfPasswords))
			{
				string passwordPart;
				do { passwordPart = GenerateSimplePasswordPart(options); }
				while (passwordPart.Length < options.MinimumLength);

				yield return passwordPart;
			}
		}

		private string GenerateSimplePasswordPart(RandomPasswordGeneratorOptions options)
		{
		  if (_random.Value == null)
				throw new InvalidOperationException("Random number generator is not initialized.");

			List<char> letters = GenerateLetters(options.NumberOfLetters).ToList();
			IEnumerable<string> numbers = GenerateNumbers(options.NumberOfNumbers);
			IEnumerable<string> symbols = GenerateSymbols(options.NumberOfSpecialCharacters);

			// The password always starts with the first letter, then: the rest of the letters, numbers, and symbols in a random order.
			IEnumerable<string> strings = new string[] { letters[0].ToString() }
			  .Concat(letters.Skip(1).Select(l => l.ToString())
				  .Concat(numbers).Concat(symbols)
				  .Where(s => s.Length > 0)
				  .OrderBy(_ => _random.Value.Next())
				);

			return string.Join(string.Empty, strings);
		}
	}
}
