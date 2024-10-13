using PG.Data.Files.Dictionaries;
using System.Diagnostics;
using System.Text;

namespace PG.Tests.Data.Dictionaries
{
	[TestClass()]
	public class TextDictionaryFileTests
	{
		[DataTestMethod]
		[DataRow(@"Resources\Dictionaries\words_alpha_enUS.txt")]
		[DataRow(@"Resources\Dictionaries\words_alpha_esES.txt")]
		public void GetWordsTest(string relativePathToFile)
		{
			string filePath = Path.Combine(Environment.CurrentDirectory, relativePathToFile);
			IDictionariesData dictionary = new DictionariesDataFactory().CreateForFile(filePath, Encoding.UTF8);
			Assert.IsTrue(dictionary.FetchAllWords().Any());
		}

		[TestMethod]
		public void ExceptionsTest()
		{
			TextDictionaryFile dictionary;

			dictionary = new("non-existent file.txt", Encoding.UTF8);
			try
			{
				_ = dictionary.FetchAllWords().Any();
				Assert.Fail("Expected exception 'Dictionary file path was not provided.' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }

			dictionary = new("", Encoding.UTF8);
			try
			{
				_ = dictionary.FetchAllWords().Any();
				Assert.Fail("Expected exception 'Dictionary file path was not provided.' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
		}
	}
}