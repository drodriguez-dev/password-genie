using Microsoft.VisualStudio.TestTools.UnitTesting;
using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Extensions;
using System.Diagnostics;
using System.Text;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public class DictionaryPasswordGeneratorTests
	{
		[DataTestMethod]
		[DataRow(8, 2, 4, 2, 2)]
		[DataRow(4, 2, 2, 0, 0)]
		[DataRow(12, 2, 6, 0, 0)]
		public void PasswordGenerationTest(int minLength, int numberOfWords, int averageWordLength, int numberOfNumbers, int numberOfSpecials)
		{
			DictionaryPasswordGeneratorOptions options = new()
			{
				File = @".\Resources\Dictionaries\words_alpha_esES.txt",
				NumberOfPasswords = 10,
				NumberOfWords = numberOfWords,
				AverageWordLength = averageWordLength,
				NumberOfNumbers = numberOfNumbers,
				NumberOfSpecialCharacters = numberOfSpecials,
				MinimumLength = minLength,
				IncludeSetSymbols = true,
				IncludeMarkSymbols = true,
				IncludeSeparatorSymbols = true,
				RemoveHighAsciiCharacters = true
			};

			IDictionariesData data = new DictionariesDataFactory().CreateForFile(options.File, Encoding.UTF8);

			Debug.WriteLine("Starting password generation...");
			DictionaryPasswordGenerator passwordGenerator = new(options, data);
			var passwords = passwordGenerator.Generate().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in passwords)
			{
				Debug.WriteLine($"  {passwordPart}");

				Assert.IsTrue(minLength <= passwordPart.Length, "Password length does not match the minimum length requirement.");
				if (numberOfWords > 0)
					Assert.IsTrue(passwordPart.Any(char.IsLetter), $"There are no letters in the password: {passwordPart}");
				if (numberOfNumbers > 0)
					Assert.IsTrue(passwordPart.Any(char.IsDigit), $"There are no numbers in the password: {passwordPart}");
				if (numberOfSpecials > 0)
					Assert.IsTrue(passwordPart.Any(c => !char.IsLetterOrDigit(c)), $"There are no special characters in the password: {passwordPart}");
			}
		}

		[TestMethod]
		public void SuggestedGenerationTest()
		{
			DictionaryPasswordGeneratorOptions options = new()
			{
				File = @".\Resources\Dictionaries\words_alpha_esES.txt",
				NumberOfPasswords = 10,
				NumberOfWords = 2,
				AverageWordLength = 6,
				NumberOfNumbers = 1,
				NumberOfSpecialCharacters = 1,
				MinimumLength = 12,
				CustomSpecialCharacters = " ".ToCharArray(),
				RemoveHighAsciiCharacters = true
			};

			IDictionariesData data = new DictionariesDataFactory().CreateForFile(options.File, Encoding.UTF8);

			Debug.WriteLine("Starting password generation...");
			DictionaryPasswordGenerator passwordGenerator = new(options, data);
			var passwords = passwordGenerator.Generate().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in passwords)
			{
				Debug.WriteLine($"  {passwordPart}");

				Assert.IsTrue(options.MinimumLength <= passwordPart.Length, "Password length does not match the minimum length requirement.");
				if (options.NumberOfWords > 0)
					Assert.IsTrue(passwordPart.Any(char.IsLetter), $"There are no letters in the password: {passwordPart}");
				if (options.NumberOfNumbers > 0)
					Assert.IsTrue(passwordPart.Any(char.IsDigit), $"There are no numbers in the password: {passwordPart}");
				if (options.NumberOfSpecialCharacters > 0)
					Assert.IsTrue(passwordPart.Any(c => !char.IsLetterOrDigit(c)), $"There are no special characters in the password: {passwordPart}");
			}
		}
	}
}