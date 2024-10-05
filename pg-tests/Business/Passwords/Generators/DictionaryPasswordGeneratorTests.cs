using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loader;
using PG.Shared.Services;
using PG.Tests.Business.Passwords.Generators.Mockups;
using System.Diagnostics;
using System.Text;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public class DictionaryPasswordGeneratorTests : PasswordGeneratorTestBase
	{
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
			var result = passwordGenerator.Generate();

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
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

			Debug.WriteLine($"Password entropy is: {0:N2}", result.AverageEntropy);
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
			var result = passwordGenerator.Generate();

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
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

			Debug.WriteLine($"Password entropy is: {0:N2}", result.AverageEntropy);
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
				var result = new DictionaryPasswordGenerator(options, new RandomService(), loader).Generate();

				Debug.WriteLine($"  {order}: {string.Join(", ", result.Passwords)}");

				Assert.IsTrue(result.Passwords.All(p => p.Length >= options.MinimumLength), "Password length does not match the minimum length requirement.");
				Assert.IsTrue(result.Passwords.All(p => WordPattern().Matches(p).Count == options.NumberOfWords), "Password does not have the expected number of words.");

				if (new[] { KeystrokeOrder.AlternatingStroke, KeystrokeOrder.AlternatingWord }.Contains(options.KeystrokeOrder))
					Assert.IsTrue(result.Passwords.All(p => LeftHandPattern().IsMatch(p) && RightHandPattern().IsMatch(p)), "Password does not contains both left and right hand keystrokes.");

				if (options.KeystrokeOrder == KeystrokeOrder.OnlyLeft)
					Assert.IsTrue(!result.Passwords.Any(RightHandPattern().IsMatch), "Password should not contain left hand keystrokes only.");

				if (options.KeystrokeOrder == KeystrokeOrder.OnlyRight)
					Assert.IsTrue(!result.Passwords.Any(LeftHandPattern().IsMatch), "Password should not contain right hand keystrokes only.");

				Debug.WriteLine($"Password entropy is: {0:N2}", result.AverageEntropy);
			}
		}

		[TestMethod]
		public void ExceptionsTest()
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
				CustomSpecialCharacters = " ".ToCharArray(),
				RemoveHighAsciiCharacters = true,
				KeystrokeOrder = KeystrokeOrder.AlternatingStroke,
			};

			IDictionaryLoader loader = new WordDictionaryLoader(new DictionariesDataFactory().CreateForFile(options.File, Encoding.UTF8));

			Debug.WriteLine("Starting password generation for exceptions...");
			try
			{
				options.NumberOfPasswords = 0;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), loader).Generate();
				Assert.Fail("Expected exception 'At least one password must be requested' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }

			try
			{
				options.NumberOfPasswords = 1;
				options.NumberOfWords = 0;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), loader).Generate();
				Assert.Fail("Expected exception 'At least one word must be requested' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }

			try
			{
				options.NumberOfWords = 1;
				options.AverageWordLength = 2;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), loader).Generate();
				Assert.Fail("Expected exception 'Average length must be at least X' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }

			try
			{
				options.NumberOfWords = 2;
				options.AverageWordLength = 6;
				options.DepthLevel = 8;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), loader).Generate();
				Assert.Fail("Expected exception 'Depth level must be lower than the average word length (X)' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }

			try
			{
				options.NumberOfWords = 1;
				options.DepthLevel = 3;
				options.MinimumLength = 12;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), loader).Generate();
				Assert.Fail("Expected exception 'Minimum length must be lower to the sum of the number of letters, numbers, and special characters (X)' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }

			try
			{
				options.NumberOfWords = 2;
				loader = new WordDictionaryLoader(new DictionaryDataMockup(["qwertasdfgzxcvb", "yuiophjklnm"]));
				_ = new DictionaryPasswordGenerator(options, new RandomService(), loader).Generate();
				Assert.Fail("Expected exception 'Max iterations reached without being able to generate a valid word.' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
		}
	}
}