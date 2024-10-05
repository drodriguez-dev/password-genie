using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Services;
using System.Diagnostics;

namespace PG.Tests.Business.Passwords.Generators
{
	[TestClass()]
	public class RandomPasswordGeneratorTests : PasswordGeneratorTestBase
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
			var result = passwordGenerator.Generate();

			Debug.WriteLine($"Generated passwords:");
			foreach (var passwordPart in result.Passwords)
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

			Debug.WriteLine($"Password entropy is: {0:N2}", result.AverageEntropy);
		}

		/// <summary>
		/// Tests the entropy calculation for different password configurations. The entropy should be the log2 of the number of possible combinations.
		/// </summary>
		/// <remarks>
		/// Because of the nature of the entropy calculation, the entropy is not an exact value.Only for very small cases (e.g. 2 letters, 1 number) the 
		/// entropy is an exact value.
		/// </remarks>
		[TestMethod]
		public void EntropyTest()
		{
			RandomPasswordGeneratorOptions options = new()
			{
				NumberOfPasswords = 10,
				//NumberOfLetters = 0,
				//NumberOfNumbers = 1,
				NumberOfSpecialCharacters = 0,
				MinimumLength = 0,
				IncludeSetSymbols = true,
				IncludeMarkSymbols = true,
				IncludeSeparatorSymbols = true,
				RemoveHighAsciiCharacters = true,
				KeystrokeOrder = KeystrokeOrder.Random,
			};

			Debug.WriteLine("Starting password generation...");

			double[] entropy = new double[4];
			int index = -1;

			options.NumberOfLetters = 0;
			options.NumberOfNumbers = 1;
			entropy[++index] = GenerateAndGetEntropy(options);
			Assert.IsTrue(entropy[index] > 0, "Entropy is not greater than 0.");
			Assert.IsTrue(CheckEntropy(entropy[index], Math.Pow(10, 1)), "Entropy should be precisely the log2 of the number of possible combinations.");

			options.NumberOfLetters = 0;
			options.NumberOfNumbers = 2;
			entropy[++index] = GenerateAndGetEntropy(options);
			Assert.IsTrue(entropy[index] > 0, "Entropy is not greater than 0.");
			Assert.IsTrue(CheckEntropy(entropy[index], Math.Pow(10, 2)), "Entropy should be precisely the log2 of the number of possible combinations.");
			Assert.IsTrue(entropy[index] > entropy[index - 1], "Entropy should be higher than the previous.");

			options.NumberOfNumbers = 1;
			options.NumberOfLetters = 2;
			entropy[++index] = GenerateAndGetEntropy(options);
			Assert.IsTrue(entropy[index] > 0, "Entropy is not greater than 0.");
			Assert.IsTrue(CheckEntropy(entropy[index], Math.Pow(52, 2) * 10 * 2), "Entropy should be precisely the log2 of the number of possible combinations.");
			Assert.IsTrue(entropy[index] > entropy[index - 1], "Entropy should be higher than the previous.");
		}

		private static double GenerateAndGetEntropy(RandomPasswordGeneratorOptions options)
		{
			RandomPasswordGenerator passwordGenerator = new(options, new RandomService());
			var result = passwordGenerator.Generate();

			Debug.WriteLine($"Entropy for '{options}': {result.AverageEntropy}");

			return result.AverageEntropy;
		}

		public static bool CheckEntropy(double entropy, double combinations)
		{
			const double precision = 0.0001d;

			double difference = Math.Abs(entropy - Math.Log2(combinations));
			return difference < precision;
		}

		[TestMethod]
		public void AlternatingHandsTest()
		{
			RandomPasswordGeneratorOptions options = new()
			{
				NumberOfPasswords = 10,
				NumberOfLetters = 8,
				NumberOfNumbers = 1,
				NumberOfSpecialCharacters = 1,
				MinimumLength = 10,
				CustomSpecialCharacters = " ".ToCharArray(),
				RemoveHighAsciiCharacters = true,
			};

			Debug.WriteLine("Starting password generation...");

			// For each keystroke order, generate a password
			foreach (KeystrokeOrder order in Enum.GetValues(typeof(KeystrokeOrder)))
			{
				options.KeystrokeOrder = order;
				var result = new RandomPasswordGenerator(options, new RandomService()).Generate();

				Debug.WriteLine($"  {order}: {string.Join(", ", result.Passwords)}");

				Assert.IsTrue(result.Passwords.All(p => p.Length >= options.MinimumLength), "Password length does not match the minimum length requirement.");
				Assert.IsTrue(result.Passwords.All(p => LettersPattern().Matches(p).Count == options.NumberOfLetters), "Password does not have the expected number of letters.");

				if (options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
					Assert.IsTrue(result.Passwords.All(p => LeftHandPattern().IsMatch(p) && RightHandPattern().IsMatch(p)), "Password does not contains both left and right hand keystrokes.");

				if (options.KeystrokeOrder == KeystrokeOrder.OnlyLeft)
					Assert.IsTrue(!result.Passwords.Any(RightHandPattern().IsMatch), "Password should not contain left hand keystrokes only.");

				if (options.KeystrokeOrder == KeystrokeOrder.OnlyRight)
					Assert.IsTrue(!result.Passwords.Any(LeftHandPattern().IsMatch), "Password should not contain right hand keystrokes only.");

				Debug.WriteLine($"Password entropy is: {0:N2}", result.AverageEntropy);
			}
		}
	}
}