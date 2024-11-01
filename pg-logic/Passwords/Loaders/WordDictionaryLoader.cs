using PG.Data.Files.DataFiles.Dictionaries;
using PG.Entities.WordTrees;
using PG.Shared.Extensions;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Loaders
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

		/// <summary>
		/// Loads the dictionary from the specified file path into a tree-like structure.
		/// </summary>
		/// <param name="dictionaryFilePath">Full path to the dictionary file.</param>
		/// <returns>Root node of the dictionary tree structure representing the dictionary.</returns>
		public WordDictionaryTree Load(Stream file)
		{
			WordDictionaryTree @return = new();

			foreach (var word in DictionariesData.FetchAllWords(file))
			{
				// Skip words that are too short
				if (word.Length < MINIMUM_WORD_LENGTH) continue;
				// Skip words that contains symbols or numbers
				if (word.Any(c => !char.IsLetter(c))) continue;
				// Skip words that contains only diacritics
				if (word.ToLowerInvariant().All(VOWEL_AND_DIACRITIC_CHARS.Contains)) continue;

				AddWordToTree(@return.Root, word);
			}

			if (@return.Root.Children.Count == 0)
				throw new InvalidDictionaryException($"Dictionary file does not contain any valid words. Words must be at least {MINIMUM_WORD_LENGTH} characters long and contain only letters.");

			return @return;
		}

		private static void AddWordToTree(TreeRoot<string> root, string word)
		{
			ITreeNode<string> node = root;

			foreach (var letter in word.GetTextElements())
				node = GetOrCreateChildNode(node, letter);
		}

		private static TreeNode<string> GetOrCreateChildNode(ITreeNode<string> node, string letter)
		{
			if (node.Children.TryGetValue(letter, out var childNode))
				return childNode;

			TreeNode<string> newNode = new(letter);
			node.Children.Add(letter, newNode);
			return newNode;
		}
	}
}
