using PG.Logic.Common;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Services;

namespace PG.Logic.Passwords.Generators
{
	public abstract class PasswordGeneratorBase(RandomService random) : IPasswordGenerator
	{
		protected readonly RandomService _random = random;
		private readonly List<double> _entropyValues = [];

		protected abstract bool IncludeSetSymbols { get; }
		protected abstract bool IncludeMarkSymbols { get; }
		protected abstract bool IncludeSeparatorSymbols { get; }
		protected abstract char[] CustomSpecialChars { get; }
		protected abstract bool RemoveHighAsciiCharacters { get; }
		protected abstract KeystrokeOrder KeystrokeOrder { get; }

		// Symbol sets
		protected static readonly char[] _setSymbols = @"()[]{}<>".ToCharArray();
		protected static readonly char[] _markSymbols = @"!@#$%^*+=|;:\""?".ToCharArray();
		protected static readonly char[] _separatorSymbols = @" -_/\&,.".ToCharArray();

		// Characters that are typed with the left hand in the US keyboard layout.
		protected static readonly char[] _leftHandKeyStrokes = @" 12345qwertasdfgzxcvbQWERTASDFGZXCVB~`!@#$%".ToCharArray();
		protected static readonly char[] _leftThumbKeyStrokes = @" ".ToCharArray();
		protected static readonly char[] _leftPinkyKeyStrokes = @"1qazQAZ~`!".ToCharArray();
		protected static readonly char[] _leftRingKeyStrokes = @"2wsxWSX@".ToCharArray();
		protected static readonly char[] _leftMiddleKeyStrokes = @"3edcEDC#".ToCharArray();
		protected static readonly char[] _leftIndexKeyStrokes = @"45rfvRFVtgbTGB$%".ToCharArray();

		// Characters that are typed with the right hand in the US keyboard layout.
		protected static readonly char[] _rightHandKeyStrokes = @" 67890yuiophjklnmYUIOPHJKLNM^&*()-_+={}[]|\:;""'<>,.?/".ToCharArray();
		protected static readonly char[] _rightThumbKeyStrokes = @" ".ToCharArray();
		protected static readonly char[] _rightPinkyKeyStrokes = @"0pP)-_+={}[]|\:;""'?/".ToCharArray();
		protected static readonly char[] _rightRingKeyStrokes = @"9olOL(.>".ToCharArray();
		protected static readonly char[] _rightMiddleKeyStrokes = @"8ikIK*,<".ToCharArray();
		protected static readonly char[] _rightIndexKeyStrokes = @"67ujmUJM^&".ToCharArray();

		public virtual GenerationResult Generate()
		{
			List<string> passwords = [];
			foreach (var passwordPart in GeneratePasswordParts())
				passwords.Add(passwordPart);

			return new GenerationResult()
			{
				Passwords = [.. passwords],
				AverageEntropy = GetAndResetPasswordEntropy()
			};
		}

		protected abstract IEnumerable<string> GeneratePasswordParts();

		protected abstract string BuildPasswordPart();

		protected virtual IEnumerable<string> BuildPasswordParts(int numberOfPasswords, int minimumLength)
		{
			foreach (int _ in Enumerable.Range(0, numberOfPasswords))
			{
				string passwordPart;
				int iterations = 0;
				_random.ResetEntropy();
				do
				{
					// If the previous password part was not valid, discard the entropy and try again.
					_random.DiscardEntropy();
					passwordPart = BuildPasswordPart();
				}
				while (iterations++ < Constants.MAX_ITERATIONS && passwordPart.Length < minimumLength);

				if (iterations >= Constants.MAX_ITERATIONS)
					throw new InvalidOperationException("Could not generate a password with the required length and the current settings.");

				_random.CommitEntropy();
				_entropyValues.Add(_random.GetBitsOfEntropy());

				yield return passwordPart;
			}
		}

		/// <summary>
		/// Generate a random string of characters.
		/// </summary>
		protected IEnumerable<string> GenerateNumbers(int length)
		{
			foreach (int _ in Enumerable.Range(0, length))
				yield return _random.Next(0, 10).ToString();

			_random.CommitEntropy();
		}

		/// <summary>
		/// Generates a random set of symbols
		/// </summary>
		protected IEnumerable<string> GenerateSymbols(int length)
		{
			if (length <= 0) yield break;

			char[] symbols = GetAvailableSymbols().ToArray();
			if (symbols.Length == 0)
				throw new InvalidOperationException("No symbols are available. Either provide custom symbols or enable the default ones.");

			foreach (int _ in Enumerable.Range(0, length))
				yield return symbols[_random.Next(symbols.Length)].ToString();

			_random.CommitEntropy();
		}

		/// <summary>
		/// Chooses between the custom or the default set and returns the available symbols.
		/// </summary>
		protected IEnumerable<char> GetAvailableSymbols()
		{
			IEnumerable<char> symbols = CustomSpecialChars;
			if (!symbols.Any())
			{
				if (IncludeSetSymbols)
					symbols = symbols.Concat(_setSymbols);

				if (IncludeMarkSymbols)
					symbols = symbols.Concat(_markSymbols);

				if (IncludeSeparatorSymbols)
					symbols = symbols.Concat(_separatorSymbols);
			}

			return symbols;
		}

		protected HandSide ChooseFirstHand()
		{
			return KeystrokeOrder switch
			{
				KeystrokeOrder.Random => HandSide.Any,
				KeystrokeOrder.OnlyLeft => HandSide.Left,
				KeystrokeOrder.OnlyRight => HandSide.Right,
				KeystrokeOrder.AlternatingWord or KeystrokeOrder.AlternatingStroke => _random.Next(2) == 0 ? HandSide.Left : HandSide.Right,
				_ => throw new NotImplementedException($"The keystroke order {KeystrokeOrder} is not implemented."),
			};
		}

		protected abstract HandSide ChooseHand(HandSide currentHand);

		/// <summary>
		/// Determines if the keystroke is a proper keystroke for the current hand. It will always return true if the keystroke order is random or the hand is any.
		/// </summary>
		protected bool IsProperHand(char keystroke, HandSide hand)
		{
			return KeystrokeOrder == KeystrokeOrder.Random
				|| (hand == HandSide.Any)
				|| (hand == HandSide.Left ? _leftHandKeyStrokes.Contains(keystroke) : _rightHandKeyStrokes.Contains(keystroke));
		}

		/// <summary>
		/// Determines if the keystroke is a proper keystroke for the current finger and hand.
		/// </summary>
		protected static bool IsProperFinger(char value, HandSide curHand, Finger? curFinger)
		{
			// If the finger is not set, any keystroke is valid.
			if (curFinger == null) return true;

			// Remove the keystrokes that are not allowed for the current finger and test if the keystroke is valid.
			return curHand switch
			{
				HandSide.Left => curFinger switch
				{
					Finger.Thumb => _leftHandKeyStrokes.Except(_leftThumbKeyStrokes).Contains(value),
					Finger.Pinky => _leftHandKeyStrokes.Except(_leftPinkyKeyStrokes).Contains(value),
					Finger.Ring => _leftHandKeyStrokes.Except(_leftRingKeyStrokes).Contains(value),
					Finger.Middle => _leftHandKeyStrokes.Except(_leftMiddleKeyStrokes).Contains(value),
					Finger.Index => _leftHandKeyStrokes.Except(_leftIndexKeyStrokes).Contains(value),
					_ => true
				},
				HandSide.Right => curFinger switch
				{
					Finger.Thumb => _rightHandKeyStrokes.Except(_rightThumbKeyStrokes).Contains(value),
					Finger.Pinky => _rightHandKeyStrokes.Except(_rightPinkyKeyStrokes).Contains(value),
					Finger.Ring => _rightHandKeyStrokes.Except(_rightRingKeyStrokes).Contains(value),
					Finger.Middle => _rightHandKeyStrokes.Except(_rightMiddleKeyStrokes).Contains(value),
					Finger.Index => _rightHandKeyStrokes.Except(_rightIndexKeyStrokes).Contains(value),
					_ => true
				},
				_ => true,
			};
		}

		protected static Finger? GetFingerForKeystroke(char value)
		{
			if (_leftThumbKeyStrokes.Contains(value)) return (Finger?)Finger.Thumb;
			if (_leftPinkyKeyStrokes.Contains(value)) return (Finger?)Finger.Pinky;
			if (_leftRingKeyStrokes.Contains(value)) return (Finger?)Finger.Ring;
			if (_leftMiddleKeyStrokes.Contains(value)) return (Finger?)Finger.Middle;
			if (_leftIndexKeyStrokes.Contains(value)) return (Finger?)Finger.Index;
			if (_rightThumbKeyStrokes.Contains(value)) return (Finger?)Finger.Thumb;
			if (_rightPinkyKeyStrokes.Contains(value)) return (Finger?)Finger.Pinky;
			if (_rightRingKeyStrokes.Contains(value)) return (Finger?)Finger.Ring;
			if (_rightMiddleKeyStrokes.Contains(value)) return (Finger?)Finger.Middle;
			if (_rightIndexKeyStrokes.Contains(value)) return (Finger?)Finger.Index;

			return null;
		}

		protected double GetAndResetPasswordEntropy()
		{
			double entropy = _entropyValues.Sum() / _entropyValues.Count;
			_entropyValues.Clear();

			return entropy;
		}
	}
}
