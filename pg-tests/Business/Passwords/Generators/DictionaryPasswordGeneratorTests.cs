using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.Dictionaries;
using PG.Entities.Files;
using PG.Entities.WordTrees;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loaders;
using PG.Shared.Services;
using PG.Tests.Business.Passwords.Generators.Mockups;
using System.Text;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public class DictionaryPasswordGeneratorTests : PasswordGeneratorTestBase
	{
		/// <summary>
		/// Tolerance for the entropy difference between true and derived entropy.
		/// </summary>
		private const double ENTROPY_TOLERANCE = 0.05;

		/// <summary>
		/// Maximum depth level for the word tree used in the tests.
		/// </summary>
		/// <remarks>
		/// Increasing this value over 4 will increase the memory usage significantly.
		/// </remarks>
		private const int MAX_DEPTH_LEVEL = 4;

		public required TestContext TestContext { get; set; }

		private static WordDictionaryTree? sharedWordTree;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			// Create a shared word tree with all possible combinations (a-z) of the max depth level plus one letter to avoid existing words causing problems.
			sharedWordTree = GetAbcWordTree(MAX_DEPTH_LEVEL + 1);
		}

		[DataTestMethod]
		[DataRow(1, 4, 0, 0)]
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

			TestContext.WriteLine("Starting password generation...");
			DictionaryPasswordGeneratorV1 passwordGenerator = new(options, new RandomService(), GetWordTree(options.File));
			var result = passwordGenerator.Generate();

			TestContext.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
			{
				TestContext.WriteLine($"  {passwordPart.Password}");
				TestContext.WriteLine("True entropy is: {0:N2} ({1})", passwordPart.TrueEntropy, passwordPart.TrueStrength);
				TestContext.WriteLine("Derived entropy is: {0:N2} ({1})", passwordPart.DerivedEntropy, passwordPart.DerivedStrength);

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

			TestContext.WriteLine("Starting password generation...");
			DictionaryPasswordGeneratorV1 passwordGenerator = new(options, new RandomService(), GetWordTree(options.File));
			var result = passwordGenerator.Generate();

			TestContext.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
			{
				TestContext.WriteLine($"  {passwordPart.Password}");
				TestContext.WriteLine("True entropy is: {0:N2} ({1})", passwordPart.TrueEntropy, passwordPart.TrueStrength);
				TestContext.WriteLine("Derived entropy is: {0:N2} ({1})", passwordPart.DerivedEntropy, passwordPart.DerivedStrength);

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

			TestContext.WriteLine("Starting password generation...");

			// For each keystroke order, generate a password
			options.KeystrokeOrder = order;
			var result = new DictionaryPasswordGeneratorV1(options, new RandomService(), GetWordTree(options.File)).Generate();
			var passwords = result.Passwords.Select(p => p.Password).ToList();

			TestContext.WriteLine(string.Join(Environment.NewLine, passwords));

			Assert.IsTrue(passwords.All(p => WordPattern().Matches(p).Count == options.NumberOfWords), "Password does not have the expected number of words.");

			if (new[] { KeystrokeOrder.AlternatingStroke, KeystrokeOrder.AlternatingWord }.Contains(options.KeystrokeOrder))
				Assert.IsTrue(passwords.All(p => LeftHandPattern().IsMatch(p) && RightHandPattern().IsMatch(p)), "Password does not contains both left and right hand keystrokes.");

			if (options.KeystrokeOrder == KeystrokeOrder.OnlyLeft)
				Assert.IsTrue(!passwords.Any(RightHandPattern().IsMatch), "Password should contain left hand keystrokes only.");

			if (options.KeystrokeOrder == KeystrokeOrder.OnlyRight)
				Assert.IsTrue(!passwords.Any(LeftHandPattern().IsMatch), "Password should contain right hand keystrokes only.");
		}

		[DataTestMethod]
		[DataRow(04, 2, 6)]
		[DataRow(05, 2, 5)]
		[DataRow(06, 3, 4)]
		[DataRow(07, 3, 3)]
		[DataRow(08, 3, 2)]
		[DataRow(09, 4, 1)]
		[DataRow(10, 4, 1)]
		[DataRow(11, 4, 1)]
		[DataRow(12, 4, 1)]
		// TODO - 2025-04-06 - Uncomment when the problem with depth level is fixed
		//[DataRow(13, 4, 1)]
		//[DataRow(14, 5, 1)]
		//[DataRow(15, 5, 1)]
		//[DataRow(16, 5, 1)]
		//[DataRow(17, 5, 1)]
		//[DataRow(18, 5, 1)]
		//[DataRow(19, 5, 1)]
		//[DataRow(20, 5, 1)]
		public void AverageWordLengthTest(int averageWordLength, int depthLevel, int numberOfWords)
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
				KeystrokeOrder = KeystrokeOrder.AlternatingStroke,
			};

			TestContext.WriteLine("Starting password generation...");
			DictionaryPasswordGeneratorV1 passwordGenerator = new(options, new RandomService(), GetWordTree(options.File));
			var result = passwordGenerator.Generate();

			TestContext.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
			{
				TracePassword(passwordPart);

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

			TestContext.WriteLine("Starting password generation for exceptions...");
			try
			{
				options.NumberOfPasswords = 0;
				_ = new DictionaryPasswordGeneratorV1(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'At least one password must be requested' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.NumberOfPasswords = 1;
				options.NumberOfWords = 0;
				_ = new DictionaryPasswordGeneratorV1(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'At least one word must be requested' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.NumberOfWords = 1;
				options.AverageWordLength = 2;
				_ = new DictionaryPasswordGeneratorV1(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'Average length must be at least X' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.DepthLevel = 8;
				_ = new DictionaryPasswordGeneratorV1(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'Depth level must be lower than the average word length (X)' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.NumberOfSpecialCharacters = 12;
				options.IncludeMarkSymbols = false;
				options.IncludeSeparatorSymbols = false;
				options.IncludeSetSymbols = false;
				options.CustomSpecialCharacters = [];
				_ = new DictionaryPasswordGeneratorV1(options, new RandomService(), wordTree).Generate();
				Assert.Fail("Expected exception 'No symbols are available. Either provide custom symbols or enable the default ones.' not thrown");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				var twoWordsTree = new WordDictionaryLoader(new DictionaryDataMockup(["qwertasdfgzxcvb", "yuiophjklnm"])).Load(null!);
				_ = new DictionaryPasswordGeneratorV1(options, new RandomService(), twoWordsTree).Generate();
				Assert.Fail("Expected exception 'Max iterations reached without being able to generate a valid word.' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }
		}

		[DataTestMethod]
		[DataRow(04, 3, DisplayName = "Entropy(wl: 04, dl: 3)")]
		[DataRow(05, 3, DisplayName = "Entropy(wl: 05, dl: 3)")]
		[DataRow(06, 3, DisplayName = "Entropy(wl: 06, dl: 3)")]
		[DataRow(07, 4, DisplayName = "Entropy(wl: 07, dl: 4)")]
		[DataRow(08, 4, DisplayName = "Entropy(wl: 08, dl: 4)")]
		[DataRow(09, 4, DisplayName = "Entropy(wl: 09, dl: 4)")]
		[DataRow(10, 4, DisplayName = "Entropy(wl: 10, dl: 4)")]
		[DataRow(11, 4, DisplayName = "Entropy(wl: 11, dl: 4)")]
		// Depth level greater than MAX_DEPTH_LEVEL is not supported (see class initialization)
		[DataRow(12, 4, DisplayName = "Entropy(wl: 12, dl: 4)")]
		[DataRow(13, 4, DisplayName = "Entropy(wl: 13, dl: 4)")]
		[DataRow(14, 4, DisplayName = "Entropy(wl: 14, dl: 4)")]
		[DataRow(15, 4, DisplayName = "Entropy(wl: 15, dl: 4)")]
		[DataRow(16, 4, DisplayName = "Entropy(wl: 16, dl: 4)")]
		[DataRow(17, 4, DisplayName = "Entropy(wl: 17, dl: 4)")]
		[DataRow(18, 4, DisplayName = "Entropy(wl: 18, dl: 4)")]
		[DataRow(19, 4, DisplayName = "Entropy(wl: 19, dl: 4)")]
		[DataRow(20, 4, DisplayName = "Entropy(wl: 20, dl: 4)")]
		[DataRow(21, 4, DisplayName = "Entropy(wl: 21, dl: 4)")]
		[DataRow(22, 4, DisplayName = "Entropy(wl: 22, dl: 4)")]
		[DataRow(23, 4, DisplayName = "Entropy(wl: 23, dl: 4)")]
		[DataRow(24, 4, DisplayName = "Entropy(wl: 24, dl: 4)")]
		[DataRow(25, 4, DisplayName = "Entropy(wl: 25, dl: 4)")]
		[DataRow(26, 4, DisplayName = "Entropy(wl: 26, dl: 4)")]
		[DataRow(27, 4, DisplayName = "Entropy(wl: 27, dl: 4)")]
		[DataRow(28, 4, DisplayName = "Entropy(wl: 28, dl: 4)")]
		[DataRow(29, 4, DisplayName = "Entropy(wl: 29, dl: 4)")]
		[DataRow(30, 4, DisplayName = "Entropy(wl: 30, dl: 4)")]
		public void EntropyTest(int wordLength, int depth)
		{
			if (sharedWordTree == null)
				throw new InvalidOperationException("Shared word tree is not initialized.");

			if (depth > MAX_DEPTH_LEVEL)
				throw new ArgumentOutOfRangeException(nameof(depth), $"Depth level cannot be greater than {MAX_DEPTH_LEVEL}.");

			DictionaryPasswordGeneratorOptions options = new()
			{
				Type = DictionaryType.PlainTextDictionary,
				File = null!, // File stream is not used in this test, a word tree is provided directly.
				NumberOfPasswords = 1,
				NumberOfWords = 1,
				AverageWordLength = wordLength,
				DepthLevel = depth,
				NumberOfNumbers = 0,
				NumberOfSpecialCharacters = 0,
				CustomSpecialCharacters = [],
				RemoveHighAsciiCharacters = false,
				KeystrokeOrder = KeystrokeOrder.Random,
			};

			TestContext.WriteLine("Starting password generation for entropy calculation...");
			var result = new DictionaryPasswordGeneratorV1(options, new RandomService(), sharedWordTree).Generate();

			TestContext.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
			{
				TracePassword(passwordPart);

				// With a word tree with all the possible combinations, the true and derived entropy should be roughly the same.
				// Assert the entropy difference based on a percentage of the derived entropy.
				Assert.AreEqual(passwordPart.TrueEntropy, passwordPart.DerivedEntropy, passwordPart.DerivedEntropy * ENTROPY_TOLERANCE,
					$"True entropy is not similar (± {ENTROPY_TOLERANCE * 100}%) to derived entropy: {passwordPart.Password}");
			}
		}

		private void TracePassword(PasswordResult passwordPart)
		{
			TestContext.WriteLine($"  {passwordPart.Password}");
			TestContext.WriteLine("True entropy is: {0:N2} ({1})", passwordPart.TrueEntropy, passwordPart.TrueStrength);
			TestContext.WriteLine("Derived entropy is: {0:N2} ({1})", passwordPart.DerivedEntropy, passwordPart.DerivedStrength);
		}

		private static WordDictionaryTree GetWordTree(Stream fileStream)
		{
			IDictionariesData data = new DictionariesDataFactory().CreateForDictionaryFile(DictionaryType.PlainTextDictionary, Encoding.UTF8);
			WordDictionaryTree wordTree = new WordDictionaryLoader(data).Load(fileStream);
			return wordTree;
		}

		/// <summary>
		/// Creates a plain dictionary with 26^4 combinations (a-z) and implement various tests with an expected entropy
		/// </summary>
		/// <remarks>
		/// This dictionary generation is very memory intensive, a depth of 4 will create 456976 nodes (91 MB)
		/// </remarks>
		private static WordDictionaryTree GetAbcWordTree(int depth)
		{
			WordDictionaryTree wordTree = new();
			AddAbcNodes(wordTree.Root, depth - 1);

			return wordTree;
		}

		private static void AddAbcNodes(ITreeNode<string> node, int depth)
		{
			foreach (char c in "abcdefghijklmnopqrstuvwxyz")
			{
				var childNode = new TreeNode<string>(c.ToString());
				node.Children.Add(c.ToString(), childNode);

				if (depth > 0)
					AddAbcNodes(childNode, depth - 1);
			}
		}
	}
}