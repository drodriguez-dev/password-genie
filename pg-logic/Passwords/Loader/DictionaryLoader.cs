using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Loader.Entities;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Loader
{
	internal class DictionaryLoader(IDictionariesData data)
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

		public WordDictionary WordDictionary { get; set; } = new();

		/// <summary>
		/// Loads the dictionary from the specified file path into a tree-like structure.
		/// </summary>
		/// <param name="dictionaryFilePath">Full path to the dictionary file.</param>
		/// <returns>Root node of the dictionary tree structure representing the dictionary.</returns>
		public void Load(string dictionaryFilePath)
		{
			if (!File.Exists(dictionaryFilePath))
				throw new FileNotFoundException($"Dictionary file not found: {dictionaryFilePath}");

			WordDictionary = new WordDictionary();

			foreach (var word in DictionariesData.FetchAllWords())
			{
				// Skip words that are too short
				if (word.Length < MINIMUM_WORD_LENGTH) continue;
				// Skip words that contains symbols or numbers
				if (word.Any(c => !char.IsLetter(c))) continue;
				// Skip words that contains only diacritics
				if (word.ToLowerInvariant().All(c => VOWEL_AND_DIACRITIC_CHARS.Contains(c))) continue;

				WordDictionary.Words.Add(word);
				AddWordToTree(WordDictionary.Root, word);
			}

			if (WordDictionary.Root.Children.Count == 0)
				throw new InvalidDictionaryException($"Dictionary file '{dictionaryFilePath}' does not contain any valid words. Words must be at least {MINIMUM_WORD_LENGTH} characters long and contain only letters.");
		}

		private static void AddWordToTree(TreeRoot<char> root, string word)
		{
			ITreeNodeWithChildren<char> node = root;

			foreach (var letter in word)
			{
				node = GetOrCreateChildNode(node, letter);
			}
		}

		private static TreeNode<char> GetOrCreateChildNode(ITreeNodeWithChildren<char> node, char letter)
		{
			if (node.Children.TryGetValue(letter, out var childNode))
				return childNode;

			TreeNode<char> newNode = new(letter);
			node.Children.Add(letter, newNode);
			return newNode;
		}
	}
}
