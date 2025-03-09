using System.Numerics;

namespace PG.Shared.Services
{
	/// <summary>
	/// Service to calculate the entropy of a password.
	/// </summary>
	/// <remarks>
	/// The entropy of the password based on the count of characters for each type and the pool of characters on the generated password. For each common 
	/// symbol type (lower case letters, upper case letters, numbers, etc.), it counts how many characters of that type there are in your password. It 
	/// returns the number of bits of entropy is the password.
	/// </remarks>
	public class EntropyService
	{
		private const int AlphabetSize = 26;
		private const int NumericSize = 10;
		private const int SymbolSize = 32;

		/// <summary>
		/// Counts the number of characters of a given type in a password and returns the number of bits of entropy taking into account the possible 
		/// combinations for each character pool.
		/// </summary>
		/// <remarks>
		/// Characters with diacrictics count as the base character. For example, "á" counts as "a".
		/// </remarks>
		public static double CalculatePasswordEntropy(string password)
		{
			BigInteger possibleCombinations = 1;
			foreach (char character in password)
			{
				char baseLetter = character;
				if (char.GetUnicodeCategory(character) == System.Globalization.UnicodeCategory.NonSpacingMark)
					baseLetter = character.ToString().Normalize(System.Text.NormalizationForm.FormD)[0];

			  if (char.IsLower(baseLetter) || char.IsUpper(baseLetter))
					possibleCombinations *= AlphabetSize;
				else if (char.IsDigit(baseLetter))
					possibleCombinations *= NumericSize;
				else if (char.IsSymbol(baseLetter) || char.IsPunctuation(baseLetter))
					possibleCombinations *= SymbolSize;
			}

			// The base 2 logarithm of the number of possible combinations is the entropy of the password.
			return BigInteger.Log(possibleCombinations, 2);
		}
	}
}
