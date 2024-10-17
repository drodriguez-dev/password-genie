using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.Dictionaries;
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
			using FileStream file = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			IDictionariesData dictionary = new DictionariesDataFactory().CreateForDictionaryFile(DictionaryType.PlainTextDictionary, Encoding.UTF8);
			Assert.IsTrue(dictionary.FetchAllWords(file).Any());
		}

		[TestMethod]
		public void ExceptionsTest()
		{
			TextDictionaryFile dictionary = new(Encoding.UTF8);

			try
			{
				_ = dictionary.FetchAllWords(null!).Any();
				Assert.Fail("Expected exception 'Dictionary file path was not provided.' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }

			try
			{
				_ = dictionary.FetchAllWords(new MemoryStream()).Any();
				Assert.Fail("Expected exception 'Dictionary file path was not provided.' not thrown.");
			}
			catch (Exception ex) { Debug.WriteLine($"Expected exception:\n  {ex}"); }
		}
	}
}