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
					HandSide curHand = currentHand;
					var children = node.Children.Select(kvp => kvp.Value)
							.Where(tn => !RemoveHighAsciiCharacters || tn.Value[0] < 128)
							.Where(tn => IsProperHand(tn.Value[0], curHand))
							.Where(tn => IsProperFinger(tn.Value[0], curHand, curFinger))
							.Where(tn => !IsInLastChars(tn.Value[0], wordBuilder.ToString(), depthLevel))
							.ToList();

					if (children.Count == 0) break;

					var next = children[_random.Next(children.Count)];
					wordBuilder = wordBuilder.Append(next.Value);

					curFinger = GetFingerForKeystroke(next.Value);

					if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
						currentHand = currentHand == HandSide.Left ? HandSide.Right : HandSide.Left;

					node = next;

					// If the word is already in the dictionary, break the loop.
					if (wordBuilder.Length >= depthLevel && !TrySearchLastPossibleLeafNode(wordBuilder.ToString(), depthLevel, out node))
						break;
				}
			} while (wordBuilder.Length < wordLength && iterations++ < Constants.MAX_ITERATIONS);

			if (iterations >= Constants.MAX_ITERATIONS)
				throw new InvalidOperationException("Max iterations reached without being able to generate a valid word. Try to reduce the restrictions.");

			return CapitalizeWord(wordBuilder.ToString());
		}
	}
}
