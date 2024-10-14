﻿using PG.Data.Files.Dictionaries;
using PG.Data.Files.WordTrees;
using PG.Entities.WordTrees;
using PG.Logic.Passwords.Loader;
using System.Text;

namespace PG.Tests.Data.WordTrees
{
	[TestClass()]
	public class WordTreeFileTests
	{

		[DataTestMethod]
		[DataRow(@"Resources\Dictionaries\words_alpha_enUS.txt")]
		[DataRow(@"Resources\Dictionaries\words_alpha_esES.txt")]
		public void LoadDictionaryAndSaveAndLoadWordTreeTest(string relativePathToFile)
		{
			long dictionarySize = new FileInfo(relativePathToFile).Length;
			System.Diagnostics.Debug.WriteLine($"Dictionary file size is {dictionarySize,9} bytes");

			WordDictionaryTree wordTree = FetchWordTree(relativePathToFile);

			string wordTreeFilePath = Path.GetTempFileName();
			try
			{
				BinaryWordTreeFile file = new(wordTreeFilePath);
				file.SaveTree(wordTree);

				long wordTreeSize = new FileInfo(wordTreeFilePath).Length;
				long difference = (wordTreeSize - dictionarySize) * 100 / dictionarySize;
				System.Diagnostics.Debug.WriteLine($"Word tree file is       {wordTreeSize,9} bytes ({difference:+#;-#;0} %)");

				WordDictionaryTree actualWordTree = file.FetchTree();

				Assert.IsTrue(actualWordTree != null, "WordTree should've been created");
				Assert.IsTrue(actualWordTree.Root != null, "Root node should've been created");
				Assert.IsTrue(actualWordTree.Root.Children.Count > 0, "Root node should have children");

				Assert.AreEqual(wordTree, actualWordTree, "Word trees should be equal");
			}
			finally
			{
				if (File.Exists(wordTreeFilePath))
					File.Delete(wordTreeFilePath);
			}
		}

		private static WordDictionaryTree FetchWordTree(string relativePathToFile)
		{
			string filePath = Path.Combine(Environment.CurrentDirectory, relativePathToFile);
			IDictionariesData dictionary = new DictionariesDataFactory().CreateForFile(filePath, Encoding.UTF8);
			WordDictionaryLoader loader = new(dictionary);
			loader.Load();

			return loader.WordTree;
		}
	}
}