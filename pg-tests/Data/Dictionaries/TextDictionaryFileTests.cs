using PG.Data.Files.Dictionaries;
using System.Text;

namespace PG.Tests.Data.Dictionaries
{
	[TestClass()]
	public class TextDictionaryFileTests
	{
		[DataTestMethod]
		[DataRow(@"Resources\Dictionaries\words_alpha_enUS.txt")]
		[DataRow(@"Resources\Dictionaries\words_alpha_esES.txt")]
		public void GetWordsTest(string relativePathToDictionary)
		{
			string filePath = Path.Combine(Environment.CurrentDirectory, relativePathToDictionary);
			if (!File.Exists(filePath))
				Assert.Fail($"File not found: {filePath}");

			IDictionariesData dictionary = new DictionariesDataFactory().CreateForFile(filePath, Encoding.UTF8);
			Assert.IsTrue(dictionary.FetchAllWords().Any());
		}
	}
}