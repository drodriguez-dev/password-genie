using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Loader;
using System.Text;

namespace PG.Tests.Business.Passwords.Loader
{
	[TestClass()]
	public class WordDictionaryLoaderTests
	{
		[DataTestMethod]
		[DataRow(@"Resources\Dictionaries\words_alpha_enUS.txt")]
		[DataRow(@"Resources\Dictionaries\words_alpha_esES.txt")]
		public void LoadDictionaryTest(string relativePathToFile)
		{
			string filePath = Path.Combine(Environment.CurrentDirectory, relativePathToFile);
			IDictionariesData dictionary = new DictionariesDataFactory().CreateForFile(filePath, Encoding.UTF8);
			WordDictionaryLoader loader = new(dictionary);
			loader.Load();

			Assert.IsTrue(loader.WordTree != null, "WordTree should've been created");
			Assert.IsTrue(loader.WordTree.Root != null, "Root node should've been created");
			Assert.IsTrue(loader.WordTree.Root.Children.Count > 0, "Root node should have children");
		}
	}
}