using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Services;
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
			RandomPasswordGenerator passwordGenerator = new(options, new RandomService());
			var passwords = passwordGenerator.Generate().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

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

		[TestMethod]
		public void EntropyTest()
		{

			RandomPasswordGeneratorOptions options = new()
			{
				NumberOfPasswords = 10,
				NumberOfLetters = 0,
				NumberOfNumbers = 1,
				NumberOfSpecialCharacters = 0,
				MinimumLength = 0,
				IncludeSetSymbols = true,
				IncludeMarkSymbols = true,
				IncludeSeparatorSymbols = true,
				RemoveHighAsciiCharacters = true
			};

			Debug.WriteLine("Starting password generation...");

			double[] entropy = new double[4];
			int curEntropy = -1;
			entropy[++curEntropy] = GenerateAndGetEntropy(options);
			Assert.IsTrue(entropy[curEntropy] > 0, "Entropy is not greater than 0.");
			Assert.IsTrue(CheckEntropy(entropy[curEntropy], 10 * 1), "Entropy should be precisely the log2 of the number of possible passwords.");

			options.NumberOfNumbers = 2;
			entropy[++curEntropy] = GenerateAndGetEntropy(options);
			Assert.IsTrue(entropy[curEntropy] > 0, "Entropy is not greater than 0.");
			Assert.IsTrue(CheckEntropy(entropy[curEntropy], Math.Pow(10, 2) * 2 * 1), "Entropy should be precisely the log2 of the number of possible passwords.");
			Assert.IsTrue(entropy[curEntropy] > entropy[curEntropy - 1], "Entropy should be higher than the previous.");

			options.NumberOfNumbers = 1;
			options.NumberOfLetters = 2;
			entropy[++curEntropy] = GenerateAndGetEntropy(options);
			Assert.IsTrue(entropy[curEntropy] > 0, "Entropy is not greater than 0.");
			Assert.IsTrue(CheckEntropy(entropy[curEntropy], Math.Pow(52, 2) * 10 * 2 * 1), "Entropy should be precisely the log2 of the number of possible passwords.");
			Assert.IsTrue(entropy[curEntropy] > entropy[curEntropy - 1], "Entropy should be higher than the previous.");

			options.NumberOfNumbers = 2;
			options.NumberOfLetters = 2;
			entropy[++curEntropy] = GenerateAndGetEntropy(options);
			Assert.IsTrue(entropy[curEntropy] > 0, "Entropy is not greater than 0.");
			Assert.IsTrue(CheckEntropy(entropy[curEntropy], Math.Pow(52, 2) * Math.Pow(10, 2) * 3 * 2 * 1), "Entropy should be precisely the log2 of the number of possible passwords.");
			Assert.IsTrue(entropy[curEntropy] > entropy[curEntropy - 1], "Entropy should be higher than the previous.");
		}

		private static double GenerateAndGetEntropy(RandomPasswordGeneratorOptions options)
		{
			RandomPasswordGenerator passwordGenerator = new(options, new RandomService());
			_ = passwordGenerator.Generate();

			var entropy = passwordGenerator.GetAndResetPasswordEntropy();
			Debug.WriteLine($"Entropy for '{options}': {entropy}");

			return entropy;
		}

		public static bool CheckEntropy(double entropy, double combinations)
		{
			const double precision = 0.0001d;

			double difference = Math.Abs(entropy - Math.Log2(combinations));
			return difference < precision;
		}

	}
}