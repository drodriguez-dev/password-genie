using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.Dictionaries;
using PG.Entities.Files;
using PG.Entities.WordTrees;
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
			using FileStream file = new(filePath, FileMode.Open, FileAccess.Read);
			IDictionariesData dictionary = new DictionariesDataFactory().CreateForDictionaryFile(DictionaryType.PlainTextDictionary, Encoding.UTF8);
			WordDictionaryLoader loader = new(dictionary);
			WordDictionaryTree wordTree = loader.Load(file);

			Assert.IsTrue(wordTree != null, "WordTree should've been created");
			Assert.IsTrue(wordTree.Root != null, "Root node should've been created");
			Assert.IsTrue(wordTree.Root.Children.Count > 0, "Root node should have children");
		}
	}
}