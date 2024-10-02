using PG.Shared.Extensions;

namespace PG.Tests.Shared.Extensions
{
	[TestClass()]
	public class TextExtensionsTests
	{
		[TestMethod()]
		public void IsPrintableTest()
		{
			Assert.IsTrue('c'.IsPrintable(), "c is printable");
			Assert.IsFalse(' '.IsPrintable(), "space is not printable");
			Assert.IsFalse('\b'.IsPrintable(), "backspace is not printable");
		}

		[TestMethod()]
		public void IsWhitespaceTest()
		{
			Assert.IsTrue(' '.IsWhitespace(), "space is whitespace");
			Assert.IsTrue('\t'.IsWhitespace(), "tab is whitespace");
			Assert.IsFalse('c'.IsWhitespace(), "c is not whitespace");
		}

		[TestMethod()]
		public void RightTest()
		{
			try { 
				"".Right(0);
				Assert.Fail("Expected ArgumentOutOfRangeException");
			}
			catch (ArgumentOutOfRangeException) { /* Expected */ }

			string? actual = null;
#pragma warning disable CS8604 // Possible null reference argument. For testing nullability
			Assert.AreEqual(null, actual.Right(1), "Right of 1 is empty");
#pragma warning restore CS8604 // Possible null reference argument.
			Assert.AreEqual("123", "abc123".Right(3), "Right of 3 is 123");
		}
	}
}