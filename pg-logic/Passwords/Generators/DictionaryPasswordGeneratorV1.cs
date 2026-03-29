using PG.Entities.WordTrees;
using PG.Logic.Common;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Services;
using System.Text;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGeneratorV1(DictionaryPasswordGeneratorOptions options, RandomService random, WordDictionaryTree wordTree)
		: DictionaryPasswordGeneratorBase(options, random, wordTree)
	{
		/// <summary>
		/// Generates a word based on the dictionary provided. Word length is variable depending on the average word length, it's variance half of the 
		/// average length.
		/// </summary>
		/// <remarks>
		/// Uses a tree structure based on the dictionary to generate fictitious but language-like words.
		/// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "<Pending>")]
		protected override string GenerateWord(int wordLength, int depthLevel, ref HandSide currentHand)
		{
			// TODO - 2025-04-06 - Refactor this method to use a more efficient algorithm.
			var wordBuilder = new StringBuilder();

			ITreeNode<string> startNode = _wordTree?.Root
				?? throw new InvalidOperationException("The word tree has not been initialized.");

			int iterations = 0;
			do
			{
				Finger? curFinger = null;
				ITreeNode<string> node = startNode;

				// If the previos word was not valid, discard the entropy and try again.
				_random.DiscardEntropy();
				wordBuilder.Clear();

				for (int index = 0; index < wordLength; index++)
				{
					int workingDepth = depthLevel;
					List<TreeNode<string>> children;
					do
					{
						// While the word is still shorter than the minimum depth level, continue with the next node.
						// Once the minimum depth level is reached, use the node from the last <depth level> amount of characters.
						if (wordBuilder.Length >= Constants.MIN_DEPTH_LEVEL)
							node = FindLastPossibleLeafNode(wordBuilder.ToString(), workingDepth);
						int requiredDepth = Math.Min(wordLength - index, workingDepth);

						HandSide curHand = currentHand;
						children = node.Children.Select(kvp => kvp.Value)
							.Where(tn => !RemoveHighAsciiCharacters || tn.Value[0] < 128)
							.Where(tn => IsProperHand(tn.Value[0], curHand))
							.Where(tn => IsProperFinger(tn.Value[0], curHand, curFinger))
							// Filter out characters that are in the last chars of the current word to avoid repetition inside the depth level.
							.Where(tn => !IsInLastChars(tn.Value[0], wordBuilder.ToString(), depthLevel))
							// Filter out nodes that do not have enough depth to reach the desired word length.
							.Where(tn => tn.MaxDepth + 1 >= requiredDepth)
							.ToList();

					} while (children.Count == 0 && wordBuilder.Length >= Constants.MIN_DEPTH_LEVEL && workingDepth-- > Constants.MIN_DEPTH_LEVEL);
					// If there are no children, it means that the current path is not valid. Reduce the path until a valid one is found or the minimum depth level is reached.

					if (children.Count == 0) break;

					var next = children[_random.Next(children.Count)];
					wordBuilder = wordBuilder.Append(next.Value);

					curFinger = GetFingerForKeystroke(next.Value);

					if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
						currentHand = currentHand == HandSide.Left ? HandSide.Right : HandSide.Left;

					node = next;
				}
			} while (isBelowTargetLength(wordBuilder.Length, wordLength) && iterations++ < Constants.MAX_ITERATIONS);

			if (iterations >= Constants.MAX_ITERATIONS)
				throw new InvalidOperationException("Max iterations reached without being able to generate a valid word. Try to reduce the restrictions.");

			return CapitalizeWord(wordBuilder.ToString());

			static bool isBelowTargetLength(int current, int target) => current < target;
		}
	}
}
