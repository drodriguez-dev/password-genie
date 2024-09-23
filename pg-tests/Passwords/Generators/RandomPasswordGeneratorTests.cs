using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using System.Diagnostics;

namespace PG.Tests.Passwords.Generators
{
	[TestClass()]
	public class RandomPasswordGeneratorTests
	{
		[DataTestMethod]
		[DataRow(8, 4, 2, 2)]
		[DataRow(10, 0, 1, 3)]
		[DataRow(12, 8, 0, 1)]
		[DataRow(12, 8, 1, 0)]
		public void PasswordGenerationTest(int minLength, int numberOfLetters, int numberOfNumbers, int numberOfSpecials)
		{
			RandomPasswordGeneratorOptions options = new()
			{
				NumberOfPasswords = 1,
				NumberOfLetters = numberOfLetters,
				NumberOfNumbers = numberOfNumbers,
				NumberOfSpecialCharacters = numberOfSpecials,
				MinimumLength = minLength,
				IncludeSetSymbols = true,
				IncludeMarkSymbols = true,
				IncludeSeparatorSymbols = true,
				RemoveHighAsciiCharacters = true
			};

			Debug.WriteLine("Starting password generation...");
			RandomPasswordGenerator passwordGenerator = new(options);
			var passwords = passwordGenerator.Generate().Split(Environment.NewLine);

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in passwords)
			{
				Debug.WriteLine($"  {passwordPart}");

				Assert.AreEqual(minLength, passwordPart.Length, "Password length does not match the minimum length requirement.");
				if (numberOfLetters > 0)
					Assert.IsTrue(passwordPart.Any(char.IsLetter), "There are no letters in the password.");
				if (numberOfNumbers > 0)
					Assert.IsTrue(passwordPart.Any(char.IsDigit), "There are no numbers in the password.");
				if (numberOfSpecials > 0)
					Assert.IsTrue(passwordPart.Any(char.IsSymbol), "There are no special characters in the password.");
			}
		}
	}
}