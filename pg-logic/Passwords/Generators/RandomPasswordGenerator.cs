using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Extensions;
using System.Text;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Generators
{
	public class RandomPasswordGenerator(RandomPasswordGeneratorOptions options) : PasswordGeneratorBase
	{
		private readonly RandomPasswordGeneratorOptions _options = options;
		protected override bool IncludeSetSymbols => _options.IncludeSetSymbols;
		protected override bool IncludeMarkSymbols => _options.IncludeMarkSymbols;
		protected override bool IncludeSeparatorSymbols => _options.IncludeSeparatorSymbols;
		protected override char[] CustomSpecialChars => _options.CustomSpecialCharacters;
		protected override bool RemoveHighAsciiCharacters => _options.RemoveHighAsciiCharacters;

		public override string Generate()
		{
			StringBuilder passwords = new();
			foreach (var passwordPart in GeneratePasswordParts())
				passwords.AppendLine(passwordPart);

			// Convert the StringBuilder to a string and remove the last line break
			return passwords.ToString().Remove(passwords.Length - Environment.NewLine.Length, Environment.NewLine.Length);
		}

		private IEnumerable<string> GeneratePasswordParts()
		{
			if (_options.NumberOfPasswords < 0)
				throw new InvalidOptionException("At least one password must be requested");

			int totalCharacterCount = _options.NumberOfLetters + _options.NumberOfNumbers + _options.NumberOfSpecialCharacters;
			if (totalCharacterCount < 1)
				throw new InvalidOptionException("At least one character group must be included.");

			if (_options.MinimumLength > totalCharacterCount)
				throw new InvalidOptionException($"Minimum length must be lower to the sum of the number of letters, numbers, and special characters ({totalCharacterCount}).");

			return BuildPasswordParts();
		}

		private IEnumerable<string> BuildPasswordParts()
		{
			foreach (int _ in Enumerable.Range(0, _options.NumberOfPasswords))
			{
				string passwordPart;
				do { passwordPart = BuildPasswordPart(); }
				while (passwordPart.Length < _options.MinimumLength);

				yield return passwordPart;
			}
		}

		private string BuildPasswordPart()
		{
			Random random = GetRandomNumberGenerator();

			List<char> letters = GenerateLetters(_options.NumberOfLetters).ToList();
			List<char> numbers = GenerateNumbers(_options.NumberOfNumbers).SelectMany(s => s.ToCharArray()).ToList();
			List<char> symbols = GenerateSymbols(_options.NumberOfSpecialCharacters).SelectMany(s => s.ToCharArray())
				.OrderBy(c => c.IsPrintable() ? int.MinValue : int.MaxValue).ToList();
			// Symbols are ordered so non-printable characters are first

			// List of positions that will be used to determine the order of the password items. It's initialized with all the possible positions ordered from 0 to n - 1.
			List<int> positions = Enumerable.Range(0, Math.Max(0, letters.Count - 1) + numbers.Count + symbols.Count).ToList();

			// The password always starts with the first letter, then: the rest of the letters, numbers, and symbols in a random order.
			// Not printable characters will never be the last character in the password.
			IEnumerable<char> passwordElements = letters.Skip(1)
				.Concat(numbers).Concat(symbols)
				.OrderBy(c => c.IsPrintable() ? PopPosition(1, positions.Count) : PopPosition(2, positions.Count - 1));

			passwordElements = letters.Take(1).Concat(passwordElements);

			return string.Join(string.Empty, passwordElements);

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
