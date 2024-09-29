using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loader;
using PG.Shared.Services;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public partial class DictionaryPasswordGeneratorTests
	{
		[GeneratedRegex(@"\w+")]
		private static partial Regex WordPattern();
		[GeneratedRegex(@"[yuiophjklnmYUIOPHJKLNM]")]
		private static partial Regex RightHandPattern();
		[GeneratedRegex(@"[qwertasdfgzxcvbQWERTASDFGZXCVB]")]
		private static partial Regex LeftHandPattern();

		[DataTestMethod]
		[DataRow(8, 2, 4, 2, 2)]
		[DataRow(4, 2, 5, 0, 0)]
		[DataRow(12, 2, 6, 0, 0)]
		public void PasswordGenerationTest(int minLength, int numberOfWords, int averageWordLength, int numberOfNumbers, int numberOfSpecials)
		{
			DictionaryPasswordGeneratorOptions options = new()
			{
				File = @".\Resources\Dictionaries\words_alpha_esES.txt",
				NumberOfPasswords = 10,
				NumberOfWords = numberOfWords,
				AverageWordLength = averageWordLength,
				DepthLevel = 3,
				NumberOfNumbers = numberOfNumbers,
				NumberOfSpecialCharacters = numberOfSpecials,
				MinimumLength = minLength,
				IncludeSetSymbols = true,
				IncludeMarkSymbols = true,
				IncludeSeparatorSymbols = true,
				RemoveHighAsciiCharacters = true,
			};

			Debug.WriteLine("Starting password generation...");
			IDictionaryLoader loader = new WordDictionaryLoader(new DictionariesDataFactory().CreateForFile(options.File, Encoding.UTF8));
			DictionaryPasswordGenerator passwordGenerator = new(options, new RandomService(), loader);
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
				DepthLevel = 3,
				NumberOfNumbers = 1,
				NumberOfSpecialCharacters = 1,
				MinimumLength = 12,
				CustomSpecialCharacters = " -.".ToCharArray(),
				RemoveHighAsciiCharacters = true,
				KeystrokeOrder = KeystrokeOrder.AlternatingStroke,
			};

			Debug.WriteLine("Starting password generation...");
			IDictionaryLoader loader = new WordDictionaryLoader(new DictionariesDataFactory().CreateForFile(options.File, Encoding.UTF8));
			DictionaryPasswordGenerator passwordGenerator = new(options, new RandomService(), loader);
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

		[TestMethod]
		public void AlternatingHandsTest()
		{
			DictionaryPasswordGeneratorOptions options = new()
			{
				File = @".\Resources\Dictionaries\words_alpha_esES.txt",
				NumberOfPasswords = 7,
				NumberOfWords = 2,
				AverageWordLength = 6,
				DepthLevel = 3,
				NumberOfNumbers = 1,
				NumberOfSpecialCharacters = 1,
				MinimumLength = 12,
				CustomSpecialCharacters = " ".ToCharArray(),
				RemoveHighAsciiCharacters = true,
			};

			Debug.WriteLine("Starting password generation...");
			IDictionaryLoader loader = new WordDictionaryLoader(new DictionariesDataFactory().CreateForFile(options.File, Encoding.UTF8));

			// For each keystroke order, generate a password
			foreach (KeystrokeOrder order in Enum.GetValues(typeof(KeystrokeOrder)))
			{
				options.KeystrokeOrder = order;
				string[] passwords = new DictionaryPasswordGenerator(options, new RandomService(), loader).Generate()
					.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

				Debug.WriteLine($"  {order}: {string.Join(", ", passwords)}");

				Assert.IsTrue(passwords.All(p => p.Length >= options.MinimumLength), "Password length does not match the minimum length requirement.");
				Assert.IsTrue(passwords.All(p => WordPattern().Matches(p).Count == options.NumberOfWords), "Password does not have the expected number of words.");

				if (new[] { KeystrokeOrder.AlternatingStroke, KeystrokeOrder.AlternatingWord }.Contains(options.KeystrokeOrder))
					Assert.IsTrue(passwords.All(p => LeftHandPattern().IsMatch(p) && RightHandPattern().IsMatch(p)), "Password does not contains both left and right hand keystrokes.");

				if (options.KeystrokeOrder == KeystrokeOrder.OnlyLeft)
					Assert.IsTrue(!passwords.Any(RightHandPattern().IsMatch), "Password should not contain left hand keystrokes only.");

				if (options.KeystrokeOrder == KeystrokeOrder.OnlyRight)
					Assert.IsTrue(!passwords.Any(LeftHandPattern().IsMatch), "Password should not contain right hand keystrokes only.");
			}
		}
	}
}