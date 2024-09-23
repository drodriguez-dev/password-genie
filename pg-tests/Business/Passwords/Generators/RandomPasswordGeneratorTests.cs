using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using System.Diagnostics;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public class RandomPasswordGeneratorTests
	{
		[DataTestMethod]
		[DataRow(8, 4, 2, 2)]
		[DataRow(5, 0, 2, 3)]
		[DataRow(6, 1, 2, 3)]
		[DataRow(9, 8, 0, 1)]
		[DataRow(10, 8, 2, 0)]
		[DataRow(7, 0, 7, 0)]
		public void PasswordGenerationTest(int minLength, int numberOfLetters, int numberOfNumbers, int numberOfSpecials)
		{
			RandomPasswordGeneratorOptions options = new()
			{
				NumberOfPasswords = 10,
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

				Assert.IsTrue(options.MinimumLength <= passwordPart.Length, "Password length does not match the minimum length requirement.");
				if (options.NumberOfLetters > 0)
					Assert.IsTrue(passwordPart.Any(char.IsLetter), $"There are no letters in the password: {passwordPart}");
				if (options.NumberOfNumbers > 0)
					Assert.IsTrue(passwordPart.Any(char.IsDigit), $"There are no numbers in the password: {passwordPart}");
				if (options.NumberOfSpecialCharacters > 0)
					Assert.IsTrue(passwordPart.Any(c => !char.IsLetterOrDigit(c)), $"There are no special characters in the password: {passwordPart}");
			}
		}
	}
}