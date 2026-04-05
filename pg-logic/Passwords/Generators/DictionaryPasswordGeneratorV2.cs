using PG.Entities.WordTrees;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Services;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGeneratorV2(DictionaryPasswordGeneratorOptions options, RandomService random, WordDictionaryTree wordTree)
		: DictionaryPasswordGeneratorBase(options, random, wordTree)
	{

		/// <summary>
		/// Generates a word based on the dictionary provided. Word length is variable depending on the average word length, it's variance half of the 
		/// average length.
		/// </summary>
		/// <remarks>
		/// Uses a tree structure based on the dictionary to generate fictitious but language-like words.
		/// </remarks>
		protected override string GenerateWord(int wordLength, int depthLevel, ref HandSide currentHand)
		{
			Finger? startFinger = null;
			if (!IsChildrenPossible(string.Empty, wordLength, depthLevel, ref currentHand, ref startFinger, out string word, currentCombinations: 1, finalCombinations: out int finalCombinations))
				throw new InvalidOperationException("Unable to generate a valid word. Please, try to reduce the restrictions.");

			// Because method "IsChildrenPossible" is recursive, only the last call to the method will have the correct number of possibilities.
			_random.DiscardEntropy();
			_random.IncrementEntropy(finalCombinations);
			_random.CommitEntropy();

			return CapitalizeWord(word);
		}

		private bool IsChildrenPossible(string word, int length, int depthLevel, ref HandSide hand, ref Finger? finger, out string newWord, int currentCombinations, out int finalCombinations)
		{
			if (length == 0)
			{
				newWord = word;
				finalCombinations = currentCombinations;
				return true; // Recursion exit condition, no more characters to add.
			}

			// Start node is the root node or the last node found in the tree up to depth level that matches the current word.
			ITreeNode<string> startNode = FindLastPossibleLeafNode(word, depthLevel);

			// Local variables to avoid "Cannot use ref parameter inside a lambda expression".
			HandSide localHand = hand;
			Finger? localFinger = finger;

			var validChildren = startNode.Children
				.Select(c => c.Value)
				.Where(tn => !RemoveHighAsciiCharacters || tn.Value[0] < 128)
				.Where(tn => IsProperHand(tn.Value[0], localHand))
				.Where(tn => IsProperFinger(tn.Value[0], localHand, localFinger))
				.Select(tn => (
					Node: tn,
					IsPossible: IsChildrenPossible(word + tn.Value, length - 1, depthLevel, ref localHand, ref localFinger, out string newWord, currentCombinations, out int finalCombos),
					Word: newWord,
					Combinations: finalCombos
				))
				.Where(ptn => ptn.IsPossible)
				.ToList();

			if (validChildren.Count <= 0)
			{
				newWord = string.Empty;
				finalCombinations = currentCombinations;
				return false;
			}

			var (node, _, selectedWord, selectedCombinations) = validChildren[_random.Next(validChildren.Count, updateEntropy: false)];
			selectedCombinations *= validChildren.Count;

			finger = GetFingerForKeystroke(node.Value);

			if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
				hand = hand == HandSide.Left ? HandSide.Right : HandSide.Left;

			newWord = selectedWord;
			finalCombinations = selectedCombinations;
			return true;
		}
	}
}
