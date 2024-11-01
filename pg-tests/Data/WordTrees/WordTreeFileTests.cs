using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.Dictionaries;
using PG.Data.Files.DataFiles.WordTrees;
using PG.Entities.Files;
using PG.Entities.WordTrees;
using PG.Logic.Passwords.Loader;
using PG.Shared.Extensions;
using System.Diagnostics;
using System.Text;

namespace PG.Tests.Data.WordTrees
{
	[TestClass()]
	public class WordTreeFileTests
	{

		[DataTestMethod]
		[DataRow("aeiou")]
		[DataRow("áéíóú")]
		[DataRow("😀")]
		[DataRow("😀😁")]
		[DataRow("😀😁😂😃😄😅😆😇😈😉😊😋😌😍😎😏😐😑😒😓😔😕😖😗😘😙😚😛😜😝😞😟😠😡😢😣😤😥😦😧😨😩😪😫😬😭😮😯😰😱😲😳😴😵😶😷😸😹😺😻😼😽😾😿🙀🙁🙂🙃🙄🙅🙆🙇🙈🙉🙊🙋🙌🙍🙎")]
		public void StringArrayTests(string characters)
		{
			WordDictionaryTree wordTree = CreateNewMockUpWordTree(characters.GetTextElements().ToArray());

			string wordTreeFilePath = Path.ChangeExtension(Path.GetTempFileName(), "gz");
			try
			{
				using (FileStream fileStream = new(wordTreeFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
					new BinaryWordTreeFile().SaveTree(fileStream, wordTree);

				WordDictionaryTree actualWordTree;
				using (FileStream fileStream = new(wordTreeFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					actualWordTree = new BinaryWordTreeFile().FetchTree(fileStream);

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

		/// <summary>
		/// This is a test to check if specific UTF8 characters are being saved and loaded correctly.
		/// </summary>
		[DataTestMethod]
		[DataRow(0x0020, 0x007F)] // Basic Latin
		[DataRow(0x0080, 0x00FF)] // Latin-1 Supplement
		[DataRow(0x0100, 0x017F)] // Latin Extended-A
		[DataRow(0x0180, 0x024F)] // Latin Extended-B
		[DataRow(0x0250, 0x02AF)] // IPA Extensions
		[DataRow(0x02B0, 0x02FF)] // Spacing Modifier Letters
		[DataRow(0x0900, 0x097F)] // Devanagari
		[DataRow(0x16A0, 0x16FF)] // Runic
		[DataRow(0x1D00, 0x1DBF)] // Phonetic Extensions and Supplement
		[DataRow(0x3040, 0x30FF)] // Hiragana and Katakana
		[DataRow(0x10000, 0x100FF)] // Linear B Syllabary and Ideograms
		[DataRow(0x1F600, 0x1F64F)] // Emoticons (Emoji)
		public void UnicodeBlocksTests(int start, int end)
		{
			var chars = Enumerable.Range(start, end - start + 1).Select(char.ConvertFromUtf32);
			WordDictionaryTree wordTree = CreateNewMockUpWordTree(chars.ToArray());

			string wordTreeFilePath = Path.ChangeExtension(Path.GetTempFileName(), "gz");
			try
			{
				using (FileStream fileStream = new(wordTreeFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
					new BinaryWordTreeFile().SaveTree(fileStream, wordTree);

				WordDictionaryTree actualWordTree;
				using (FileStream fileStream = new(wordTreeFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					actualWordTree = new BinaryWordTreeFile().FetchTree(fileStream);

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

		private static WordDictionaryTree CreateNewMockUpWordTree(string[] chars)
		{
			Debug.WriteLine($"Recreating a word tree with {chars.Length} characters:");

			WordDictionaryTree wordTree = new();
			ITreeNode<string> node = wordTree.Root;
			for (int i = 0; i < chars.Length; i++)
			{
				string key = chars[i];
				Debug.Write(key);

				node.Children.Add(key, new TreeNode<string>(key));
			}
			Debug.WriteLine("");

			Debug.WriteLine("Word tree created:");
			WriteWordTree(wordTree.Root);

			return wordTree;
		}

		/// <summary>
		/// Runs through the tree recursively and writes the characters to the console. It writes one word for each branch.
		/// </summary>
		/// <param name="root"></param>
		/// <exception cref="NotImplementedException"></exception>
		private static void WriteWordTree(ITreeNode<string> node)
		{
			foreach (var child in node.Children)
			{
				Debug.Write(child.Key);
				WriteWordTree(child.Value);
			}
			Debug.WriteLine("");
		}

		[DataTestMethod]
		[DataRow(@"Resources\Dictionaries\words_alpha_enUS.txt")]
		[DataRow(@"Resources\Dictionaries\words_alpha_esES.txt")]
		public void LoadDictionaryAndSaveAndLoadWordTreeTest(string relativePathToFile)
		{
			long dictionarySize = new FileInfo(relativePathToFile).Length;
			long wordTreeSize;

			WordDictionaryTree wordTree = FetchWordTree(relativePathToFile);

			string wordTreeFilePath = Path.ChangeExtension(Path.GetTempFileName(), "gz");
			try
			{
				using (FileStream fileStream = new(wordTreeFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
					new BinaryWordTreeFile().SaveTree(fileStream, wordTree);

				wordTreeSize = new FileInfo(wordTreeFilePath).Length;

				WordDictionaryTree actualWordTree;
				using (FileStream fileStream = new(wordTreeFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					actualWordTree = new BinaryWordTreeFile().FetchTree(fileStream);

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

			// Display the difference in file sizes between the dictionary and the word tree files
			long difference = (wordTreeSize - dictionarySize) * 100 / dictionarySize;
			Debug.WriteLine($"Dictionary file size is {dictionarySize,9} bytes");
			Debug.WriteLine($"Word tree file is       {wordTreeSize,9} bytes ({difference:+#;-#;0} %)");
		}

		private static WordDictionaryTree FetchWordTree(string relativePathToFile)
		{
			string filePath = Path.Combine(Environment.CurrentDirectory, relativePathToFile);
			FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			IDictionariesData dictionary = new DictionariesDataFactory().CreateForDictionaryFile(DictionaryType.PlainTextDictionary, Encoding.UTF8);

			return new WordDictionaryLoader(dictionary).Load(fileStream);
		}
	}
}