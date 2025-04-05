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

		protected static readonly char[] _numbers = "0123456789".ToCharArray();

		// Symbol sets
		protected static readonly char[] _setSymbols = @"()[]{}<>".ToCharArray();
		protected static readonly char[] _markSymbols = @"!@#$%^*+=|;:\""?".ToCharArray();
		protected static readonly char[] _separatorSymbols = @" -_/\&,.".ToCharArray();

		// Characters that are typed with the left hand in the US keyboard layout.
		protected static readonly char[] _leftThumbKeyStrokes = @" ".ToCharArray();
		protected static readonly char[] _leftPinkyKeyStrokes = @"1qazQAZ~`!".ToCharArray();
		protected static readonly char[] _leftRingKeyStrokes = @"2wsxWSX@".ToCharArray();
		protected static readonly char[] _leftMiddleKeyStrokes = @"3edcEDC#".ToCharArray();
		protected static readonly char[] _leftIndexKeyStrokes = @"45rfvRFVtgbTGB$%".ToCharArray();
		protected static readonly char[] _leftHandKeyStrokes = [.. _leftThumbKeyStrokes, .. _leftPinkyKeyStrokes, .. _leftRingKeyStrokes, .. _leftMiddleKeyStrokes, .. _leftIndexKeyStrokes];

		// Characters that are typed with the right hand in the US keyboard layout.
		protected static readonly char[] _rightThumbKeyStrokes = @" ".ToCharArray();
		protected static readonly char[] _rightPinkyKeyStrokes = @"0pP)-_+={}[]|\:;""'?/".ToCharArray();
		protected static readonly char[] _rightRingKeyStrokes = @"9olOL(.>".ToCharArray();
		protected static readonly char[] _rightMiddleKeyStrokes = @"8ikIK*,<".ToCharArray();
		protected static readonly char[] _rightIndexKeyStrokes = @"67ujmUJM^&".ToCharArray();
		protected static readonly char[] _rightHandKeyStrokes = [.. _rightThumbKeyStrokes, .. _rightPinkyKeyStrokes, .. _rightRingKeyStrokes, .. _rightMiddleKeyStrokes, .. _rightIndexKeyStrokes];

		public abstract void Configure(CommonPasswordGeneratorOptions config);

		public virtual GenerationResult Generate()
		{
			List<PasswordResult> passwords = [];
			foreach (var passwordPart in GeneratePasswordParts())
			{
				PasswordResult result = new()
				{
					Password = passwordPart,
					TrueEntropy = GetAndResetPasswordEntropy(),
					DerivedEntropy = EntropyService.CalculatePasswordEntropy(passwordPart),
				};
				result.TrueStrength = CalculateStrength(result.TrueEntropy);
				result.DerivedStrength = CalculateStrength(result.DerivedEntropy);

				passwords.Add(result);
			}

			return new GenerationResult()
			{
				Passwords = [.. passwords],
			};
		}

		protected abstract IEnumerable<string> GeneratePasswordParts();

		protected abstract string BuildPasswordPart();

		protected virtual IEnumerable<string> BuildPasswordParts(int numberOfPasswords)
		{
			foreach (int _ in Enumerable.Range(0, numberOfPasswords))
			{
				_random.ResetEntropy();
				string passwordPart = BuildPasswordPart();

				_random.CommitEntropy();
				_entropyValues.Add(_random.GetBitsOfEntropy());

				yield return passwordPart;
			}
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
				KeystrokeOrder.AlternatingWord or KeystrokeOrder.AlternatingStroke => _random.Next(2, false) == 0 ? HandSide.Left : HandSide.Right,
				_ => throw new NotImplementedException($"The keystroke order {KeystrokeOrder} is not implemented."),
			};
		}

		protected abstract HandSide ChooseHand(HandSide currentHand);

		/// <summary>
		/// Determines if the keystroke is a proper keystroke for the current hand. It will always return true if the keystroke order is random or the hand is any.
		/// </summary>
		protected bool IsProperHand(char keystroke, HandSide hand)
		{
			if (char.IsSurrogate(keystroke)) return true;

			return KeystrokeOrder == KeystrokeOrder.Random
				|| (hand == HandSide.Any)
				|| (hand == HandSide.Left ? _leftHandKeyStrokes.Contains(keystroke) : _rightHandKeyStrokes.Contains(keystroke));
		}

		/// <summary>
		/// Determines if the keystroke is a proper keystroke for the current finger and hand.
		/// </summary>
		protected static bool IsProperFinger(char keystroke, HandSide curHand, Finger? curFinger)
		{
			// If the finger is not set, any keystroke is valid.
			if (curFinger == null) return true;

			if (char.IsSurrogate(keystroke)) return true;

			// Remove the keystrokes that are not allowed for the current finger and test if the keystroke is valid.
			return curHand switch
			{
				HandSide.Left => curFinger switch
				{
					Finger.Thumb => _leftHandKeyStrokes.Except(_leftThumbKeyStrokes).Contains(keystroke),
					Finger.Pinky => _leftHandKeyStrokes.Except(_leftPinkyKeyStrokes).Contains(keystroke),
					Finger.Ring => _leftHandKeyStrokes.Except(_leftRingKeyStrokes).Contains(keystroke),
					Finger.Middle => _leftHandKeyStrokes.Except(_leftMiddleKeyStrokes).Contains(keystroke),
					Finger.Index => _leftHandKeyStrokes.Except(_leftIndexKeyStrokes).Contains(keystroke),
					_ => true
				},
				HandSide.Right => curFinger switch
				{
					Finger.Thumb => _rightHandKeyStrokes.Except(_rightThumbKeyStrokes).Contains(keystroke),
					Finger.Pinky => _rightHandKeyStrokes.Except(_rightPinkyKeyStrokes).Contains(keystroke),
					Finger.Ring => _rightHandKeyStrokes.Except(_rightRingKeyStrokes).Contains(keystroke),
					Finger.Middle => _rightHandKeyStrokes.Except(_rightMiddleKeyStrokes).Contains(keystroke),
					Finger.Index => _rightHandKeyStrokes.Except(_rightIndexKeyStrokes).Contains(keystroke),
					_ => true
				},
				_ => true,
			};
		}

		protected static Finger? GetFingerForKeystroke(string value)
		{
			// This method cannot determine the finger for multi-character keystrokes.
			if (value.Length != 1) return null;

			char @char = value[0];

			if (_leftThumbKeyStrokes.Contains(@char)) return (Finger?)Finger.Thumb;
			if (_leftPinkyKeyStrokes.Contains(@char)) return (Finger?)Finger.Pinky;
			if (_leftRingKeyStrokes.Contains(@char)) return (Finger?)Finger.Ring;
			if (_leftMiddleKeyStrokes.Contains(@char)) return (Finger?)Finger.Middle;
			if (_leftIndexKeyStrokes.Contains(@char)) return (Finger?)Finger.Index;
			if (_rightThumbKeyStrokes.Contains(@char)) return (Finger?)Finger.Thumb;
			if (_rightPinkyKeyStrokes.Contains(@char)) return (Finger?)Finger.Pinky;
			if (_rightRingKeyStrokes.Contains(@char)) return (Finger?)Finger.Ring;
			if (_rightMiddleKeyStrokes.Contains(@char)) return (Finger?)Finger.Middle;
			if (_rightIndexKeyStrokes.Contains(@char)) return (Finger?)Finger.Index;

			return null;
		}

		protected double GetAndResetPasswordEntropy()
		{
			double entropy = _entropyValues.Sum() / _entropyValues.Count;
			_entropyValues.Clear();

			return entropy;
		}

		public static PasswordStrength CalculateStrength(double entropy)
		{
			return entropy switch
			{
				< 28 => PasswordStrength.VeryWeak,
				< 36 => PasswordStrength.Weak,
				< 60 => PasswordStrength.Reasonable,
				< 128 => PasswordStrength.Strong,
				_ => PasswordStrength.VeryStrong
			};
		}
	}
}
