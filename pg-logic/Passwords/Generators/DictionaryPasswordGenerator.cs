using PG.Logic.Common;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loader;
using PG.Logic.Passwords.Loader.Entities;
using PG.Shared.Extensions;
using PG.Shared.Services;
using System.Text;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Generators
{
	public class DictionaryPasswordGenerator(DictionaryPasswordGeneratorOptions options, RandomService random, IDictionaryLoader dictionaryLoader) : PasswordGeneratorBase(random)
	{
		private const int MINIMUM_AVERAGE_WORD_LENGTH = 4;
		private const int WORD_VARIANCE_DIVIDER = 2;
		private readonly IDictionaryLoader _dictionary = dictionaryLoader;

		private enum HandSide { Left, Right }
		private enum Finger { Pinky, Ring, Middle, Index, Thumb }

		private readonly DictionaryPasswordGeneratorOptions _options = options;
		protected override bool IncludeSetSymbols => _options.IncludeSetSymbols;
		protected override bool IncludeMarkSymbols => _options.IncludeMarkSymbols;
		protected override bool IncludeSeparatorSymbols => _options.IncludeSeparatorSymbols;
		protected override char[] CustomSpecialChars => _options.CustomSpecialCharacters;
		protected override bool RemoveHighAsciiCharacters => _options.RemoveHighAsciiCharacters;

		// Characters that are typed with the left hand in the US keyboard layout.
		protected static readonly char[] _leftHandKeyStrokes = @"12345qwertasdfgzxcvbQWERTASDFGZXCVB~`!@#$%".ToCharArray();
		protected static readonly char[] _leftPinkyKeyStrokes = @"1qazQAZ~`!".ToCharArray();
		protected static readonly char[] _leftRingKeyStrokes = @"2wsxWSX@".ToCharArray();
		protected static readonly char[] _leftMiddleKeyStrokes = @"3edcEDC#".ToCharArray();
		protected static readonly char[] _leftIndexKeyStrokes = @"45rfvRFVtgbTGB$%".ToCharArray();

		// Characters that are typed with the right hand in the US keyboard layout.
		protected static readonly char[] _rightHandKeyStrokes = @"67890yuiophjklnmYUIOPHJKLNM^&*()-_+={}[]|\:;""'<>,.?/".ToCharArray();
		protected static readonly char[] _rightPinkyKeyStrokes = @"0pP)-_+={}[]|\:;""'?/".ToCharArray();
		protected static readonly char[] _rightRingKeyStrokes = @"9olOL(.>".ToCharArray();
		protected static readonly char[] _rightMiddleKeyStrokes = @"8ikIK*,<".ToCharArray();
		protected static readonly char[] _rightIndexKeyStrokes = @"67ujmUJM^&".ToCharArray();

		public override GenerationResult Generate()
		{
			_dictionary.Load();

			return base.Generate();
		}

		protected override IEnumerable<string> GeneratePasswordParts()
		{
			if (_options.NumberOfPasswords < 1)
				throw new InvalidOptionException("At least one password must be requested");

			if (_options.NumberOfWords < 1)
				throw new InvalidOptionException("At least one word must be requested");

			if (_options.AverageWordLength < MINIMUM_AVERAGE_WORD_LENGTH)
				throw new InvalidOptionException($"Average length must be at least {MINIMUM_AVERAGE_WORD_LENGTH}.");

			int totalCharacterCount = (_options.AverageWordLength * _options.NumberOfWords) + _options.NumberOfNumbers + _options.NumberOfSpecialCharacters;
			if (totalCharacterCount <= 0)
				throw new InvalidOptionException("At least one character group must be included.");

			if (_options.MinimumLength > totalCharacterCount)
				throw new InvalidOptionException($"Minimum length must be lower to the sum of the number of letters, numbers, and special characters ({totalCharacterCount}).");

			if (_options.DepthLevel > _options.AverageWordLength)
				throw new InvalidOptionException($"Depth level must be lower than the average word length ({_options.DepthLevel}).");

			return BuildPasswordParts(_options.NumberOfPasswords, _options.MinimumLength);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "Too complex")]
		protected override string BuildPasswordPart()
		{
			List<string> words = GenerateWords(_options.NumberOfWords, _options.AverageWordLength, _options.DepthLevel).ToList();
			List<string> numbers = GenerateNumbers(_options.NumberOfNumbers).ToList();
			List<string> symbols = GenerateSymbols(_options.NumberOfSpecialCharacters)
				.OrderBy(s => EndsWithWhitespace(s) ? int.MinValue : int.MaxValue).ToList();

			// List of positions that will be used to determine the order of the password items. It's initialized with all the possible positions ordered from 0 to n - 1.
			List<int> positions = Enumerable.Range(0, Math.Max(0, words.Count - 1) + numbers.Count + symbols.Count).ToList();

			// The password always starts with the first word, then: the rest of the words, numbers, and symbols in a random order.
			// Not printable characters will never be the last character in the password.
			IEnumerable<string> passwordElements = words.Skip(1)
				.Concat(numbers).Concat(symbols)
				.OrderBy(s => EndsWithWhitespace(s) ? PopPosition(1, positions.Count) : PopPosition(2, positions.Count - 1));

			passwordElements = words.Take(1).Concat(passwordElements);
			string @return = string.Join(string.Empty, passwordElements);
			_random.CommitEntropy();

			return @return;

			static bool EndsWithWhitespace(string text) => text.Length > 0 && text[^1].IsWhitespace();

			// Returns a random position from the list of available positions and removes it from the list.
			int PopPosition(int min, int max)
			{
				if (positions.Count == 0) throw new InvalidOperationException("There are no more positions available.");

				// Ensure the min and max values are within the range of the positions list.
				min = Math.Max(1, Math.Min(positions.Count, min));
				max = Math.Max(1, Math.Min(positions.Count, max));

				int index = _random.Next(max - min + 1) + min - 1;
				int position = positions[index];
				positions.RemoveAt(index);

				return position;
			}
		}

		/// <summary>
		/// Generates a random set of words based on the dictionary provided.
		/// </summary>
		internal IEnumerable<string> GenerateWords(int numberOfWords, int averageLength, int depthLevel)
		{
			HandSide currentHand;
			currentHand = ChooseFirstHand();

			foreach (int _ in Enumerable.Range(0, numberOfWords))
			{
				string? word;
				int iterations = 0;

				// Generate a word that is not already in the dictionary (IsLeafNodeReached) and that does not start with any of the words in the dictionary.
				do
				{
					// If the previos word was not valid, discard the entropy and try again.
					_random.DiscardEntropy();
					word = GenerateWord(averageLength, depthLevel, ref currentHand);
				}
				while (iterations++ < Constants.MAX_ITERATIONS && string.IsNullOrEmpty(word) && _dictionary.IsLeafNodeReached(word));

				if (iterations >= Constants.MAX_ITERATIONS)
					throw new InvalidOperationException("Max iterations reached without being able to generate a valid word.");

				yield return word;

				_random.CommitEntropy();

				currentHand = ChooseHand(currentHand);
			}
		}

		private HandSide ChooseFirstHand()
		{
			HandSide currentHand;
			if (_options.KeystrokeOrder == KeystrokeOrder.OnlyLeft || _options.KeystrokeOrder == KeystrokeOrder.OnlyRight)
				currentHand = _options.KeystrokeOrder == KeystrokeOrder.OnlyLeft ? HandSide.Left : HandSide.Right;
			else
			{
				currentHand = _random.Next(2) == 0 ? HandSide.Left : HandSide.Right;
				_random.CommitEntropy();
			}

			return currentHand;
		}

		private HandSide ChooseHand(HandSide currentHand)
		{
			if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingWord)
				currentHand = currentHand == HandSide.Left ? HandSide.Right : HandSide.Left;
			return currentHand;
		}

		/// <summary>
		/// Generates a word based on the dictionary provided. Word length is variable depending on the average word length, it's variance half of the 
		/// average length.
		/// </summary>
		/// <remarks>
		/// Uses a tree structure based on the dictionary to generate fictitious but language-like words.
		/// </remarks>
		private string GenerateWord(int averageLength, int depthLevel, ref HandSide currentHand)
		{
			var wordBuilder = new StringBuilder();

			// The variance is half of the average length. For example, if the average length is 8, the variance is 4; so the word length can be
			// between 4 and 12.
			var wordLengthVariance = Math.Max(1, averageLength / WORD_VARIANCE_DIVIDER);
			int wordLength = averageLength + (_random.Next(wordLengthVariance * 2) - wordLengthVariance);

			Finger? curFinger = null;
			ITreeNodeWithChildren<char> node = _dictionary.WordTree.Root;
			foreach (int _ in Enumerable.Range(0, wordLength))
			{
				HandSide curHand = currentHand;
				var children = node.Children.Select(kvp => kvp.Value)
						.Where(tn => !RemoveHighAsciiCharacters || tn.Value < 128)
						.Where(tn => IsProperHand(tn.Value, curHand))
						.Where(tn => IsProperFinger(tn.Value, curHand, curFinger))
						.ToList();

				if (children.Count == 0) break;

				var next = children[_random.Next(children.Count)];
				wordBuilder.Append(next.Value);

				curFinger = GetFingerForKeystroke(next.Value);

				if (_options.KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
					currentHand = currentHand == HandSide.Left ? HandSide.Right : HandSide.Left;

				node = next;

				// If the word is already in the dictionary, break the loop.
				if (wordBuilder.Length >= depthLevel && !_dictionary.TrySearchLastPossibleLeafNode(wordBuilder.ToString(), depthLevel, out node))
					break;
			}

			// Return the word with the first letter capitalized.
			string word = wordBuilder.ToString();

			if (word.Length == 0) return string.Empty;
			else if (word.Length == 1) return word.ToUpper();
			else return char.ToUpper(word[0]) + word[1..];
		}

		/// <summary>
		/// Determines if the keystroke is a proper keystroke for the current hand.
		/// </summary>
		private bool IsProperHand(char keystroke, HandSide hand)
		{
			return _options.KeystrokeOrder == KeystrokeOrder.Random
				|| (hand == HandSide.Left ? _leftHandKeyStrokes.Contains(keystroke) : _rightHandKeyStrokes.Contains(keystroke));
		}

		/// <summary>
		/// Determines if the keystroke is a proper keystroke for the current finger and hand.
		/// </summary>
		private static bool IsProperFinger(char value, HandSide curHand, Finger? curFinger)
		{
			// If the finger is not set, any keystroke is valid.
			if (curFinger == null) return true;

			// Remove the keystrokes that are not allowed for the current finger and test if the keystroke is valid.
			return curHand switch
			{
				HandSide.Left => curFinger switch
				{
					Finger.Pinky => _leftHandKeyStrokes.Except(_leftPinkyKeyStrokes).Contains(value),
					Finger.Ring => _leftHandKeyStrokes.Except(_leftRingKeyStrokes).Contains(value),
					Finger.Middle => _leftHandKeyStrokes.Except(_leftMiddleKeyStrokes).Contains(value),
					Finger.Index => _leftHandKeyStrokes.Except(_leftIndexKeyStrokes).Contains(value),
					_ => true
				},
				HandSide.Right => curFinger switch
				{
					Finger.Pinky => _rightHandKeyStrokes.Except(_rightPinkyKeyStrokes).Contains(value),
					Finger.Ring => _rightHandKeyStrokes.Except(_rightRingKeyStrokes).Contains(value),
					Finger.Middle => _rightHandKeyStrokes.Except(_rightMiddleKeyStrokes).Contains(value),
					Finger.Index => _rightHandKeyStrokes.Except(_rightIndexKeyStrokes).Contains(value),
					_ => true
				},
				_ => true,
			};
		}

		private static Finger? GetFingerForKeystroke(char value)
		{
			if (_leftPinkyKeyStrokes.Contains(value)) return (Finger?)Finger.Pinky;
			if (_leftRingKeyStrokes.Contains(value)) return (Finger?)Finger.Ring;
			if (_leftMiddleKeyStrokes.Contains(value)) return (Finger?)Finger.Middle;
			if (_leftIndexKeyStrokes.Contains(value)) return (Finger?)Finger.Index;
			if (_rightPinkyKeyStrokes.Contains(value)) return (Finger?)Finger.Pinky;
			if (_rightRingKeyStrokes.Contains(value)) return (Finger?)Finger.Ring;
			if (_rightMiddleKeyStrokes.Contains(value)) return (Finger?)Finger.Middle;
			if (_rightIndexKeyStrokes.Contains(value)) return (Finger?)Finger.Index;

			return null;
		}
	}
}
