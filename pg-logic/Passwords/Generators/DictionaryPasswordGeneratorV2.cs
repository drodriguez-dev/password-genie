using PG.Entities.WordTrees;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Services;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGeneratorV2(DictionaryPasswordGeneratorOptions options, RandomService random, WordDictionaryTree wordTree)
		: DictionaryPasswordGeneratorBase(options, random, wordTree)
	{
		private class WordGenerationState
		{
			public HandSide Hand { get; set; }
			public Finger? Finger { get; set; }
			public long Combinations { get; set; }
		}

		private record struct WordGenerationResult(string Word, long Combinations = 1);

		/// <summary>
		/// Generates a word based on the dictionary provided. Word length is variable depending on the average word length, it's variance half of the 
		/// average length.
		/// </summary>
		/// <remarks>
		/// Uses a tree structure based on the dictionary to generate fictitious but language-like words.
		/// </remarks>
		protected override string GenerateWord(int wordLength, int depthLevel, ref HandSide currentHand)
		{
			if (wordLength < 3)
				throw new ArgumentException("Word length must be at least 3 characters.", nameof(wordLength));

			int firstHalfLength = wordLength / 2;
			int secondHalfLength = wordLength - firstHalfLength;

			TraverseDirection = TraverseDirection.Forwards;
			WordGenerationState state = new() { Hand = currentHand, Finger = null, Combinations = 1 };
			if (!TryGenerateWord(string.Empty, firstHalfLength, depthLevel, state, output: out WordGenerationResult result))
				throw new InvalidOperationException("Unable to generate a valid word. Please, try to reduce the restrictions.");

			string finalWord = result.Word;
			RecordEntropy(result.Combinations);

			TraverseDirection = TraverseDirection.Backwards;
			if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke && secondHalfLength % 2 == 0)
				state.Hand = state.Hand == HandSide.Left ? HandSide.Right : HandSide.Left;
			state.Finger = null;
			state.Combinations = 1;
			if (!TryGenerateWord(string.Empty, secondHalfLength, depthLevel, state, output: out result))
				throw new InvalidOperationException("Unable to generate a valid word. Please, try to reduce the restrictions.");

			finalWord += new string([.. result.Word.Reverse()]);
			RecordEntropy(result.Combinations);

			return CapitalizeWord(finalWord);
		}

		private void RecordEntropy(long combinations)
		{
			// Any previous entropy will be discarded, because method "TryGenerateWord" is recursive,
			// only the last call to the method will have the correct number of possibilities.
			_random.DiscardEntropy();
			_random.IncrementEntropy(combinations);
			_random.CommitEntropy();
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
				.ToList();

			TreeNode<string>? selectedChild = null;
			WordGenerationResult selectedResult = new(string.Empty, 1);
			while (validChildren.Count > 0 && selectedChild == null)
			{
				var tn = validChildren[_random.Next(validChildren.Count, updateEntropy: false)];

				var testState = new WordGenerationState() { Hand = state.Hand, Finger = GetFingerForKeystroke(tn.Value), Combinations = state.Combinations };
				if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
					testState.Hand = state.Hand == HandSide.Left ? HandSide.Right : HandSide.Left;

				if (TryGenerateWord(word + tn.Value, length - 1, depthLevel, testState, out WordGenerationResult result))
				{
					selectedChild = tn;
					selectedResult = result;
				}
				else
					validChildren.Remove(tn);
			}

			if (selectedChild == null)
			{
				output = default;
				return false;
			}

			selectedResult.Combinations *= validChildren.Count;

			state.Finger = GetFingerForKeystroke(selectedChild.Value);
			if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
				state.Hand = state.Hand == HandSide.Left ? HandSide.Right : HandSide.Left;
			state.Combinations = selectedResult.Combinations;

			output = new WordGenerationResult(selectedResult.Word, selectedResult.Combinations);
			return true;
		}
	}
}
