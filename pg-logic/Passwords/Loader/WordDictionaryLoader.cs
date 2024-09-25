using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Loader.Entities;
using PG.Shared.Extensions;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Loader
{
	public class WordDictionaryLoader(IDictionariesData data) : IDictionaryLoader
	{
		private const int MINIMUM_WORD_LENGTH = 2;
		private readonly HashSet<char> VOWEL_AND_DIACRITIC_CHARS = [
				'a', 'e', 'i', 'o', 'u',
				'á', 'é', 'í', 'ó', 'ú',
				'à', 'è', 'ì', 'ò', 'ù',
				'â', 'ê', 'î', 'ô', 'û',
				'ä', 'ë', 'ï', 'ö', 'ü',
				'ã', 'õ',
				'å', 'æ', 'ø',
				'ā', 'ē', 'ī', 'ō', 'ū',
				'ă', 'ĕ', 'ĭ', 'ŏ', 'ŭ',
				'ą', 'ę', 'į', 'ų',
				'ǎ', 'ě', 'ǐ', 'ǒ', 'ǔ',
				'ǟ', 'ǡ', 'ǣ',
				'ǧ', 'ǯ',
				'ǹ', 'ǻ', 'ǽ',
				'ǿ', 'ȁ', 'ȅ', 'ȉ', 'ȍ', 'ȑ', 'ȕ',
				'ȧ', 'ȩ', 'ȯ', 'ȳ',
				'ɐ', 'ɑ', 'ɒ',
				'ɓ', 'ɔ', 'ɕ', 'ɖ', 'ɗ',
				'ə', 'ɛ', 'ɜ', 'ɞ', 'ɟ', 'ɠ',
				'ɡ', 'ɢ', 'ɣ', 'ɤ', 'ɥ', 'ɦ',
				'ɧ', 'ɨ', 'ɩ', 'ɪ', 'ɫ', 'ɬ', 'ɭ', 'ɮ', 'ɯ', 'ɰ',
				'ɱ', 'ɲ', 'ɳ', 'ɴ', 'ɵ', 'ɶ',
				'ɷ', 'ɸ', 'ɹ', 'ɺ', 'ɻ', 'ɼ', 'ɽ', 'ɾ', 'ɿ',
				'ʀ', 'ʁ', 'ʂ', 'ʃ', 'ʄ', 'ʅ', 'ʆ', 'ʇ', 'ʈ', 'ʉ', 'ʊ', 'ʋ', 'ʌ', 'ʍ', 'ʎ', 'ʏ',
				'ʐ', 'ʑ', 'ʒ', 'ʓ', 'ʔ', 'ʕ', 'ʖ', 'ʗ', 'ʘ', 'ʙ', 'ʚ', 'ʛ', 'ʜ', 'ʝ', 'ʞ', 'ʟ',
				'ʠ', 'ʡ', 'ʢ', 'ʣ', 'ʤ', 'ʥ', 'ʦ', 'ʧ', 'ʨ', 'ʩ', 'ʪ', 'ʫ', 'ʬ', 'ʭ',
				];

		public IDictionariesData DictionariesData { get; set; } = data;

		public WordDictionaryTree WordTree { get; set; } = new();

		/// <summary>
		/// Loads the dictionary from the specified file path into a tree-like structure.
		/// </summary>
		/// <param name="dictionaryFilePath">Full path to the dictionary file.</param>
		/// <returns>Root node of the dictionary tree structure representing the dictionary.</returns>
		public void Load(string dictionaryFilePath)
		{
			if (!File.Exists(dictionaryFilePath))
				throw new FileNotFoundException($"Dictionary file not found: {dictionaryFilePath}");

			WordTree = new();

			foreach (var word in DictionariesData.FetchAllWords())
			{
				// Skip words that are too short
				if (word.Length < MINIMUM_WORD_LENGTH) continue;
				// Skip words that contains symbols or numbers
				if (word.Any(c => !char.IsLetter(c))) continue;
				// Skip words that contains only diacritics
				if (word.ToLowerInvariant().All(c => VOWEL_AND_DIACRITIC_CHARS.Contains(c))) continue;

				AddWordToTree(WordTree.Root, word);
			}

			if (WordTree.Root.Children.Count == 0)
				throw new InvalidDictionaryException($"Dictionary file '{dictionaryFilePath}' does not contain any valid words. Words must be at least {MINIMUM_WORD_LENGTH} characters long and contain only letters.");
		}

		private static void AddWordToTree(TreeRoot<char> root, string word)
		{
			ITreeNodeWithChildren<char> node = root;

			foreach (var letter in word)
				node = GetOrCreateChildNode(node, letter);
		}

		private static TreeNode<char> GetOrCreateChildNode(ITreeNodeWithChildren<char> node, char letter)
		{
			if (node.Children.TryGetValue(letter, out var childNode))
				return childNode;

			TreeNode<char> newNode = new(letter);
			node.Children.Add(letter, newNode);
			return newNode;
		}

		/// <summary>
		/// Searches for a leaf node in the dictionary tree by traversing the tree using the specified word and returns true if the leaf node is reached; 
		/// the word is found.
		/// </summary>
		public bool IsLeafNodeReached(string word) => TrySearchLeafNode(word, out _);

		/// <summary>
		/// Searches for a leaf node in the dictionary tree by traversing the tree using the specified word. If the word is not found, the search stops at 
		/// the last node that was found and returns false.
		/// </summary>
		private bool TrySearchLeafNode(string word, out ITreeNodeWithChildren<char> node)
		{
			node = WordTree.Root;
			foreach (var letter in word)
			{
				var children = node.Children.Select(kvp => kvp.Value)
					.FirstOrDefault(tn => tn.Value.ToString().Equals(letter.ToString(), StringComparison.InvariantCultureIgnoreCase));

				if (children == default) return false;

				node = children;
			}

			return true;
		}

		/// <summary>
		/// Searches for the last possible leaf node in the dictionary tree by successively removing the last character of the word. If there is no valid 
		/// node, the search stops at the last node that was found and returns false.
		/// </summary>
		public bool TrySearchLastPossibleLeafNode(string word, int depthLevel, out ITreeNodeWithChildren<char> node)
		{
			if (depthLevel <= 0)
				throw new ArgumentOutOfRangeException(nameof(depthLevel), "Depth level must be greater than zero.");

			bool found;
			do { found = TrySearchLeafNode(word.Right(depthLevel--), out node); }
			while (!found && depthLevel > 0);

			return found;
		}
	}
}
