using Microsoft.VisualStudio.TestTools.UnitTesting;
using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.Dictionaries;
using PG.Entities.Files;
using PG.Entities.WordTrees;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loaders;
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
		[DataRow(2, 4, 2, 2)]
		[DataRow(2, 5, 0, 0)]
		[DataRow(2, 6, 0, 0)]
		public void PasswordGenerationTest(int numberOfWords, int averageWordLength, int numberOfNumbers, int numberOfSpecials)
		{
			FileStream fileStream = new(@".\Resources\Dictionaries\words_alpha_enUS.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
			DictionaryPasswordGeneratorOptions options = new()
			{
				Type = DictionaryType.PlainTextDictionary,
				File = fileStream,
				NumberOfPasswords = 10,
				NumberOfWords = numberOfWords,
				AverageWordLength = averageWordLength,
				DepthLevel = 3,
				NumberOfNumbers = numberOfNumbers,
				NumberOfSpecialCharacters = numberOfSpecials,
				IncludeSetSymbols = true,
				IncludeMarkSymbols = true,
				IncludeSeparatorSymbols = true,
				RemoveHighAsciiCharacters = true,
			};

			Debug.WriteLine("Starting password generation...");
			DictionaryPasswordGenerator passwordGenerator = new(options, new RandomService(), GetWordTree(options.File));
			var result = passwordGenerator.Generate();

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
			{
				Debug.WriteLine($"  {passwordPart.Password}");
				Debug.WriteLine("True entropy is: {0:N2} ({1})", passwordPart.TrueEntropy, passwordPart.TrueStrength);
				Debug.WriteLine("Derived entropy is: {0:N2} ({1})", passwordPart.DerivedEntropy, passwordPart.DerivedStrength);

				if (numberOfWords > 0)
					Assert.IsTrue(passwordPart.Password.Any(char.IsLetter), $"There are no letters in the password: {passwordPart.Password}");
				if (numberOfNumbers > 0)
					Assert.IsTrue(passwordPart.Password.Any(char.IsDigit), $"There are no numbers in the password: {passwordPart.Password}");
				if (numberOfSpecials > 0)
					Assert.IsTrue(passwordPart.Password.Any(c => !char.IsLetterOrDigit(c)), $"There are no special characters in the password: {passwordPart.Password}");
			}
		}

		[TestMethod]
		public void SuggestedGenerationTest()
		{
			FileStream fileStream = new(@".\Resources\Dictionaries\words_alpha_esES.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
			DictionaryPasswordGeneratorOptions options = new()
			{
				Type = DictionaryType.PlainTextDictionary,
				File = fileStream,
				NumberOfPasswords = 10,
				NumberOfWords = 2,
				AverageWordLength = 6,
				DepthLevel = 3,
				NumberOfNumbers = 1,
				NumberOfSpecialCharacters = 1,
				CustomSpecialCharacters = " -.".ToCharArray(),
				RemoveHighAsciiCharacters = true,
				KeystrokeOrder = KeystrokeOrder.AlternatingStroke,
			};

			Debug.WriteLine("Starting password generation...");
			DictionaryPasswordGenerator passwordGenerator = new(options, new RandomService(), GetWordTree(options.File));
			var result = passwordGenerator.Generate();

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
			{
				Debug.WriteLine($"  {passwordPart.Password}");
				Debug.WriteLine("True entropy is: {0:N2} ({1})", passwordPart.TrueEntropy, passwordPart.TrueStrength);
				Debug.WriteLine("Derived entropy is: {0:N2} ({1})", passwordPart.DerivedEntropy, passwordPart.DerivedStrength);

				if (options.NumberOfWords > 0)
					Assert.IsTrue(passwordPart.Password.Any(char.IsLetter), $"There are no letters in the password: {passwordPart.Password}");
				if (options.NumberOfNumbers > 0)
					Assert.IsTrue(passwordPart.Password.Any(char.IsDigit), $"There are no numbers in the password: {passwordPart.Password}");
				if (options.NumberOfSpecialCharacters > 0)
					Assert.IsTrue(passwordPart.Password.Any(c => !char.IsLetterOrDigit(c)), $"There are no special characters in the password: {passwordPart.Password}");
			}
		}

		[DataTestMethod]
		[DataRow(KeystrokeOrder.Random)]
		[DataRow(KeystrokeOrder.AlternatingStroke)]
		[DataRow(KeystrokeOrder.AlternatingWord)]
		[DataRow(KeystrokeOrder.OnlyLeft)]
		[DataRow(KeystrokeOrder.OnlyRight)]
		public void AlternatingHandsTest(KeystrokeOrder order)
		{
			FileStream fileStream = new(@".\Resources\Dictionaries\words_alpha_esES.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
			DictionaryPasswordGeneratorOptions options = new()
			{
				Type = DictionaryType.PlainTextDictionary,
				File = fileStream,
				NumberOfPasswords = 50,
				NumberOfWords = 2,
				AverageWordLength = 6,
				DepthLevel = 3,
				NumberOfNumbers = 1,
				NumberOfSpecialCharacters = 1,
				CustomSpecialCharacters = " ".ToCharArray(),
				RemoveHighAsciiCharacters = true,
			};

			Debug.WriteLine("Starting password generation...");

			// For each keystroke order, generate a password
			options.KeystrokeOrder = order;
			var result = new DictionaryPasswordGenerator(options, new RandomService(), GetWordTree(options.File)).Generate();
			var passwords = result.Passwords.Select(p => p.Password).ToList();

			Debug.WriteLine(string.Join(Environment.NewLine, passwords));

			Assert.IsTrue(passwords.All(p => WordPattern().Matches(p).Count == options.NumberOfWords), "Password does not have the expected number of words.");

			if (new[] { KeystrokeOrder.AlternatingStroke, KeystrokeOrder.AlternatingWord }.Contains(options.KeystrokeOrder))
				Assert.IsTrue(passwords.All(p => LeftHandPattern().IsMatch(p) && RightHandPattern().IsMatch(p)), "Password does not contains both left and right hand keystrokes.");

			if (options.KeystrokeOrder == KeystrokeOrder.OnlyLeft)
				Assert.IsTrue(!passwords.Any(RightHandPattern().IsMatch), "Password should contain left hand keystrokes only.");

			if (options.KeystrokeOrder == KeystrokeOrder.OnlyRight)
				Assert.IsTrue(!passwords.Any(LeftHandPattern().IsMatch), "Password should contain right hand keystrokes only.");
		}

		[DataTestMethod]
		[DataRow(1, 04, 03)]
		[DataRow(2, 06, 05)]
		[DataRow(3, 08, 07)]
		[DataRow(4, 10, 05)]
		[DataRow(5, 12, 06)]
		[DataRow(6, 14, 07)]
		public void AverageWordLengthTest(int numberOfWords, int averageWordLength, int depthLevel)
		{
			FileStream fileStream = new(@".\Resources\Dictionaries\words_alpha_esES.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
			DictionaryPasswordGeneratorOptions options = new()
			{
				Type = DictionaryType.PlainTextDictionary,
				File = fileStream,
				NumberOfPasswords = 25,
				NumberOfWords = numberOfWords,
				AverageWordLength = averageWordLength,
				DepthLevel = depthLevel,
				NumberOfNumbers = 0,
				NumberOfSpecialCharacters = 0,
				CustomSpecialCharacters = [],
				RemoveHighAsciiCharacters = true,
			};

			Debug.WriteLine("Starting password generation...");
			DictionaryPasswordGenerator passwordGenerator = new(options, new RandomService(), GetWordTree(options.File));
			var result = passwordGenerator.Generate();

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
			{
				Debug.WriteLine($"  {passwordPart.Password}");
				Debug.WriteLine("True entropy is: {0:N2} ({1})", passwordPart.TrueEntropy, passwordPart.TrueStrength);
				Debug.WriteLine("Derived entropy is: {0:N2} ({1})", passwordPart.DerivedEntropy, passwordPart.DerivedStrength);

				Assert.AreEqual(options.NumberOfWords * options.AverageWordLength, passwordPart.Password.Length, 
					$"Password does not have the expected length: {passwordPart.Password}");
			}
		}

		[TestMethod]
		public void ExceptionsTest()
		{
			FileStream fileStream = new(@".\Resources\Dictionaries\words_alpha_esES.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
			DictionaryPasswordGeneratorOptions options = new() { Type = DictionaryType.PlainTextDictionary, File = fileStream };

			void SetDefaults()
			{
				options.NumberOfPasswords = 10;
				options.NumberOfWords = 2;
				options.AverageWordLength = 6;
				options.DepthLevel = 3;
				options.NumberOfNumbers = 1;
				options.NumberOfSpecialCharacters = 1;
				options.CustomSpecialCharacters = " ".ToCharArray();
				options.RemoveHighAsciiCharacters = true;
				options.KeystrokeOrder = KeystrokeOrder.AlternatingStroke;
			}

			SetDefaults();
			WordDictionaryTree wordTree = GetWordTree(options.File);

			Debug.WriteLine("Starting password generation for exceptions...");
			try
			{
				options.NumberOfPasswords = 0;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'At least one password must be requested' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.NumberOfPasswords = 1;
				options.NumberOfWords = 0;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'At least one word must be requested' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.NumberOfWords = 1;
				options.AverageWordLength = 2;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'Average length must be at least X' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.DepthLevel = 8;
				_ = new DictionaryPasswordGenerator(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'Depth level must be lower than the average word length (X)' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.NumberOfSpecialCharacters = 12;
				options.IncludeMarkSymbols = false;
				options.IncludeSeparatorSymbols = false;
				options.IncludeSetSymbols = false;
				options.CustomSpecialCharacters = [];
				_ = new DictionaryPasswordGenerator(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'No symbols are available. Either provide custom symbols or enable the default ones.' not thrown");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				wordTree = new WordDictionaryLoader(new DictionaryDataMockup(["qwertasdfgzxcvb", "yuiophjklnm"])).Load(null!);
				_ = new DictionaryPasswordGenerator(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'Max iterations reached without being able to generate a valid word.' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			// Be aware that at this point "wordTree" contains the tree of only two words.
		}

		private static WordDictionaryTree GetWordTree(Stream fileStream)
		{
			IDictionariesData data = new DictionariesDataFactory().CreateForDictionaryFile(DictionaryType.PlainTextDictionary, Encoding.UTF8);
			WordDictionaryTree wordTree = new WordDictionaryLoader(data).Load(fileStream);
			return wordTree;
		}
	}
}