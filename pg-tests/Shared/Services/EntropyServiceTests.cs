using PG.Shared.Services;

namespace PG.Tests.Shared.Services
{
	[TestClass]
	public class EntropyServiceTests
	{
		[DataTestMethod]
		[DataRow("a", 4.70)]
		[DataRow("á", 4.70)]
		[DataRow("1!", 8.32)]
		[DataRow("a1", 8.02)]
		[DataRow("a1!", 13.02)]
		public void CalculateStandardEntropyTest(string password, double expected)
		{
			double result = EntropyService.CalculatePasswordEntropy(password);
			Assert.AreEqual(expected, result, 0.01, $"The entropy of the password \"{password}\" is not the expected one.");
		}
	}
}