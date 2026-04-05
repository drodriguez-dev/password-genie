using PG.Entities.WordTrees;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Services;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGeneratorV2(DictionaryPasswordGeneratorOptions options, RandomService random, WordDictionaryTree wordTree)
		: DictionaryPasswordGeneratorBase(options, random, wordTree)
	{
		private record struct WordGenerationResult(string Word, int Combinations);

		private class WordGenerationState
		{
			public HandSide Hand { get; set; }
			public Finger? Finger { get; set; }
			public int Combinations { get; set; }
		}

		/// <summary>
		/// Generates a word based on the dictionary provided. Word length is variable depending on the average word length, it's variance half of the 
		/// average length.
		/// </summary>
		/// <remarks>
		/// Uses a tree structure based on the dictionary to generate fictitious but language-like words.
		/// </remarks>
		protected override string GenerateWord(int wordLength, int depthLevel, ref HandSide currentHand)
		{
			WordGenerationState initialState = new() { Hand = currentHand, Finger = null, Combinations = 1 };
			if (!TryGenerateWord(string.Empty, wordLength, depthLevel, initialState, output: out WordGenerationResult result))
				throw new InvalidOperationException("Unable to generate a valid word. Please, try to reduce the restrictions.");

			// Because method "TryGenerateWord" is recursive, only the last call to the method will have the correct number of possibilities.
			_random.DiscardEntropy();
			_random.IncrementEntropy(result.Combinations);
			_random.CommitEntropy();

			return CapitalizeWord(result.Word);
		}

		private bool TryGenerateWord(string word, int length, int depthLevel, WordGenerationState state, out WordGenerationResult output)
		{
			if (length == 0)
			{
				output = new WordGenerationResult(word, state.Combinations);
				return true; // Recursion exit condition, no more characters to add.
			}

			// Start node is the root node or the last node found in the tree up to depth level that matches the current word.
			ITreeNode<string> startNode = FindLastPossibleLeafNode(word, depthLevel);

			var validChildren = startNode.Children
				.Select(c => c.Value)
				.Where(tn => !RemoveHighAsciiCharacters || tn.Value[0] < 128)
				.Where(tn => IsProperHand(tn.Value[0], state.Hand))
				.Where(tn => IsProperFinger(tn.Value[0], state.Hand, state.Finger))
				.Select(tn => (
					Node: tn,
					IsPossible: TryGenerateWord(word + tn.Value, length - 1, depthLevel, state, out WordGenerationResult result),
					result.Word,
					result.Combinations
				))
				.Where(ptn => ptn.IsPossible)
				.ToList();

			if (validChildren.Count <= 0)
			{
				output = default;
				return false;
			}

			var (node, _, selectedWord, selectedCombinations) = validChildren[_random.Next(validChildren.Count, updateEntropy: false)];
			selectedCombinations *= validChildren.Count;

			state.Finger = GetFingerForKeystroke(node.Value);

			if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
				state.Hand = state.Hand == HandSide.Left ? HandSide.Right : HandSide.Left;

			output = new WordGenerationResult(selectedWord, selectedCombinations);
			return true;
		}
	}
}
