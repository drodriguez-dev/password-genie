using PG.Shared.Services;

namespace PG.Tests.Services
{
	[TestClass()]
	public class RandomServiceTests
	{
		private readonly RandomService _randomService = new();

		[DataTestMethod()]
		[DataRow(1, 10)]
		[DataRow(2, 10)]
		[DataRow(3, 10)]
		[DataRow(4, 10)]
		[DataRow(5, 10)]
		[DataRow(6, 10)]
		public void GetNumbersForAverageTest(int count, int average)
		{
			var numbers = _randomService.GenerateNumbersForAverage(count, average).ToList();

			Assert.IsTrue(numbers.Count == count, "Count of numbers is not equal to the requested count.");
			Assert.AreEqual(average * count, numbers.Sum(), "Sum of numbers is not equal to the requested average.");
		}
	}
}