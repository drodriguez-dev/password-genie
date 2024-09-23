using PG.Data.Files.Dictionaries;
using PG.Logic.Passwords.Loader.Entities;

namespace PG.Logic.Passwords.Loader
{
	internal class DictionaryLoader(IDictionariesData data)
	{
		private const int _minimumWordLength = 2;
		private readonly HashSet<char> _vowelsAndDiacritics = [
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

		/// <summary>
		/// Load the dictionary from the specified file path into a tree-like structure.
		/// </summary>
		/// <param name="dictionaryFullPath">Full path to the dictionary file.</param>
		/// <returns>Root node of the dictionary tree structure representing the dictionary.</returns>
		public WordDictionary Load(string dictionaryFullPath)
		{
			if (!File.Exists(dictionaryFullPath))
				throw new FileNotFoundException($"Dictionary file not found: {dictionaryFullPath}");

			var dictionary = new WordDictionary();

			foreach (var word in DictionariesData.FetchAllWords())
			{
				// Skip words that are too short
				if (word.Length < _minimumWordLength) continue;
				// Skip words that contains symbols or numbers
				if (word.Any(c => !char.IsLetter(c))) continue;
				// Skip words that contains only diacritics
				if (word.All(c => _vowelsAndDiacritics.Contains(c))) continue;

				dictionary.Words.Add(word);
				AddWordToTree(dictionary.Root, word);
			}

			if (dictionary.Words.Count == 0)
				throw new InvalidOperationException($"Dictionary file '{dictionaryFullPath}' does not contain any valid words. Words must be at least {_minimumWordLength} characters long and contain only letters.");

			return dictionary;
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
