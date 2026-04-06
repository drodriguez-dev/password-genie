using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.Dictionaries;
using PG.Entities.Files;
using PG.Entities.WordTrees;
using PG.Logic.Common;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loaders;
using PG.Shared.Services;
using PG.Tests.Business.Passwords.Generators.Mockups;
using System.Text;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public abstract class DictionaryPasswordGeneratorTestsBase<T> : PasswordGeneratorTestBase where T : DictionaryPasswordGeneratorBase
	{
		/// <summary>
		/// Tolerance for the entropy difference between true and derived entropy.
		/// </summary>
		private const double ENTROPY_TOLERANCE = 0.25;

		/// <summary>
		/// Maximum depth level for the shared word tree used in the tests.
		/// </summary>
		/// <remarks>
		/// Increasing this value over 4 will increase the memory usage significantly.
		/// </remarks>
		private const int MAX_DEPTH_LEVEL = 4;

		/// <summary>
		/// Represents the inclusive range of valid word lengths supported by the application used in some tests.
		/// </summary>
		private static readonly IEnumerable<int> WordLengthRange = Enumerable.Range(4, 20);

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
			T passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;
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
			T passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;

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
				File = null!, // File stream is not used in this test, a word tree is provided directly.
				NumberOfPasswords = 1,
				NumberOfWords = order == KeystrokeOrder.AlternatingWord ? 2 : 1,
				// AverageWordLength is setted in the loop to test multiple lengths with the same options.
				// DepthLevel depends on AverageWordLength, so it is setted in the loop as well.
				NumberOfNumbers = 0,
				NumberOfSpecialCharacters = order == KeystrokeOrder.AlternatingWord ? 1 : 0,
				CustomSpecialCharacters = " ".ToArray(),
				RemoveHighAsciiCharacters = true,
				KeystrokeOrder = order,
			};

			T passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), sharedWordTree)!;

			TestContext.WriteLine("Starting password generation...");
			List<string> passwords = [];
			foreach (int wordLength in WordLengthRange)
			{
				options.AverageWordLength = wordLength;
				int maxDepthLevel = (int)Math.Round(Math.Sqrt(options.AverageWordLength) * Constants.DEPTH_LEVEL_COEFFICIENT, digits: 0);
				options.DepthLevel = Math.Min(maxDepthLevel, MAX_DEPTH_LEVEL);
				passwordGenerator.Configure(options);
				var result = passwordGenerator.Generate();
				passwords.AddRange(result.Passwords.Select(p => p.Password));
			}

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
		[DataRow(04, 2, 7, DisplayName = "Average length 04, depth 2")]
		[DataRow(05, 2, 6, DisplayName = "Average length 05, depth 2")]
		[DataRow(06, 3, 5, DisplayName = "Average length 06, depth 3")]
		[DataRow(07, 3, 4, DisplayName = "Average length 07, depth 3")]
		[DataRow(08, 3, 3, DisplayName = "Average length 08, depth 3")]
		[DataRow(09, 4, 3, DisplayName = "Average length 09, depth 4")]
		[DataRow(10, 4, 3, DisplayName = "Average length 10, depth 4")]
		[DataRow(11, 4, 2, DisplayName = "Average length 11, depth 4")]
		[DataRow(12, 4, 2, DisplayName = "Average length 12, depth 4")]
		[DataRow(13, 4, 2, DisplayName = "Average length 13, depth 4")]
		[DataRow(14, 5, 2, DisplayName = "Average length 14, depth 5")]
		[DataRow(15, 5, 2, DisplayName = "Average length 15, depth 5")]
		[DataRow(16, 5, 1, DisplayName = "Average length 16, depth 5")]
		[DataRow(17, 5, 1, DisplayName = "Average length 17, depth 5")]
		[DataRow(18, 5, 1, DisplayName = "Average length 18, depth 5")]
		[DataRow(19, 5, 1, DisplayName = "Average length 19, depth 5")]
		[DataRow(20, 5, 1, DisplayName = "Average length 20, depth 5")]
		public void AverageWordLengthTest(int wordLength, int depthLevel, int numberOfWords)
		{
			FileStream fileStream = new(@".\Resources\Dictionaries\words_alpha_esES.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
			DictionaryPasswordGeneratorOptions options = new()
			{
				Type = DictionaryType.PlainTextDictionary,
				File = fileStream,
				NumberOfPasswords = 25,
				NumberOfWords = numberOfWords,
				AverageWordLength = wordLength,
				DepthLevel = depthLevel,
				NumberOfNumbers = 0,
				NumberOfSpecialCharacters = 0,
				CustomSpecialCharacters = [],
				RemoveHighAsciiCharacters = true,
				KeystrokeOrder = KeystrokeOrder.AlternatingStroke,
			};

			TestContext.WriteLine("Starting password generation...");
			T passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;

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
				var passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;
				_ = passwordGenerator.Generate();
				Assert.Fail("Expected exception 'At least one password must be requested' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.NumberOfPasswords = 1;
				options.NumberOfWords = 0;
				var passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;
				_ = passwordGenerator.Generate();
				Assert.Fail("Expected exception 'At least one word must be requested' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.NumberOfWords = 1;
				options.AverageWordLength = 2;
				var passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;
				_ = passwordGenerator.Generate();
				Assert.Fail("Expected exception 'Average length must be at least X' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				options.DepthLevel = 8;
				var passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;
				_ = passwordGenerator.Generate();
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
				var passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;
				_ = passwordGenerator.Generate();
				Assert.Fail("Expected exception 'No symbols are available. Either provide custom symbols or enable the default ones.' not thrown");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }

			try
			{
				var twoWordsTree = new WordDictionaryLoader(new DictionaryDataMockup(["qwertasdfgzxcvb", "yuiophjklnm"])).Load(null!);
				var passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), GetWordTree(options.File))!;
				_ = passwordGenerator.Generate();
				Assert.Fail("Expected exception 'Max iterations reached without being able to generate a valid word.' not thrown.");
			}
			catch (AssertFailedException) { throw; }
			catch (Exception ex) { TestContext.WriteLine($"Expected exception:\n  {ex}"); }
			finally { SetDefaults(); }
		}

		[DataTestMethod]
		[DataRow(04, 2, DisplayName = "Entropy for length 04, depth 2")]
		[DataRow(05, 2, DisplayName = "Entropy for length 05, depth 2")]
		[DataRow(06, 3, DisplayName = "Entropy for length 06, depth 3")]
		[DataRow(07, 3, DisplayName = "Entropy for length 07, depth 3")]
		[DataRow(08, 3, DisplayName = "Entropy for length 08, depth 3")]
		[DataRow(09, 4, DisplayName = "Entropy for length 09, depth 4")]
		[DataRow(10, 4, DisplayName = "Entropy for length 10, depth 4")]
		[DataRow(11, 4, DisplayName = "Entropy for length 11, depth 4")]
		[DataRow(12, 4, DisplayName = "Entropy for length 12, depth 4")]
		[DataRow(13, 4, DisplayName = "Entropy for length 13, depth 4")]
		// This test use a fabricated word tree of MAX_DEPTH_LEVEL combinations and does not support more than that depth.
		[DataRow(14, 4, DisplayName = "Entropy for length 14, depth 4")]
		[DataRow(15, 4, DisplayName = "Entropy for length 15, depth 4")]
		[DataRow(16, 4, DisplayName = "Entropy for length 16, depth 4")]
		[DataRow(17, 4, DisplayName = "Entropy for length 17, depth 4")]
		[DataRow(18, 4, DisplayName = "Entropy for length 18, depth 4")]
		[DataRow(19, 4, DisplayName = "Entropy for length 19, depth 4")]
		[DataRow(20, 4, DisplayName = "Entropy for length 20, depth 4")]
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
			T passwordGenerator = (T)Activator.CreateInstance(typeof(T), options, new RandomService(), sharedWordTree)!;
			var result = passwordGenerator.Generate();

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
			wordTree.Root.CalculateMaxDepth();
			wordTree.ReverseRoot = wordTree.Root;

			return wordTree;
		}

		private static void AddAbcNodes(ITreeNode<string> node, int depth)
		{
			foreach (char c in "abcdefghijklmnopqrstuvwxyz")
			{
				string letter = c.ToString();
				var childNode = new TreeNode<string>(letter);
				node.Children.Add(letter, childNode);

				if (depth > 0)
					AddAbcNodes(childNode, depth - 1);
			}
		}
	}
}