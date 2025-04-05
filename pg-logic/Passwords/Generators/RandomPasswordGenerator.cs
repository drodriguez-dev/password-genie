using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.Services;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Logic.Passwords.Generators
{
	public class RandomPasswordGenerator(RandomPasswordGeneratorOptions options, RandomService random) : PasswordGeneratorBase(random)
	{
		private RandomPasswordGeneratorOptions _options = options;
		private static int GetTotalCharacters(RandomPasswordGeneratorOptions options) => options.NumberOfLetters + options.NumberOfNumbers + options.NumberOfSpecialCharacters;

		protected override bool IncludeSetSymbols => _options.IncludeSetSymbols;
		protected override bool IncludeMarkSymbols => _options.IncludeMarkSymbols;
		protected override bool IncludeSeparatorSymbols => _options.IncludeSeparatorSymbols;
		protected override char[] CustomSpecialChars => _options.CustomSpecialCharacters;
		protected override bool RemoveHighAsciiCharacters => _options.RemoveHighAsciiCharacters;
		protected override KeystrokeOrder KeystrokeOrder => _options.KeystrokeOrder;

		protected static readonly char[] _letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

		public override void Configure(CommonPasswordGeneratorOptions config)
		{
			if (config is not RandomPasswordGeneratorOptions options)
				throw new ArgumentException($"Invalid configuration type ({config.GetType()}).", nameof(config));

			_options = options;
		}

		protected override IEnumerable<string> GeneratePasswordParts()
		{
			if (_options.NumberOfPasswords < 1)
				throw new InvalidOptionException("At least one password must be requested");

			if (GetTotalCharacters(_options) < 1)
				throw new InvalidOptionException("At least one character group must be included.");

			return BuildPasswordParts(_options.NumberOfPasswords);
		}

		protected override string BuildPasswordPart()
		{
			int letters = _options.NumberOfLetters;
			int numbers = _options.NumberOfNumbers;
			int specialCharacters = _options.NumberOfSpecialCharacters;
			HandSide currentHand = ChooseFirstHand();
			Finger? currentFinger = null;

			var password = Enumerable.Range(1, GetTotalCharacters(_options))
				.Select(i => ChooseCharacterSet(i, letters, numbers, specialCharacters))
				.Select(cs => GetAndDiscountAvailableCharacters(cs, ref letters, ref numbers, ref specialCharacters))
				.Select(aC => FilterPossibleCharacters(aC, ref currentHand, currentFinger))
				.Select(pC => ChooseCharacter(pC, ref currentFinger));

			return string.Join(string.Empty, password);
		}

		/// <summary>
		/// Returns a characters set based on the available characters and the position of the character in the password.
		/// </summary>
		/// <param name="position">Position where the new character will be inserted.</param>
		/// <param name="letters">Quantity of letters still available.</param>
		/// <param name="numbers">Quantity of numbers still available.</param>
		/// <param name="symbols">Quantity of symbols still available.</param>
		/// <returns></returns>
		private CharacterSet ChooseCharacterSet(int position, int letters, int numbers, int symbols)
		{
			IEnumerable<CharacterSet> charSets = [];

			// Build the character set based on the available characters
			if (letters > 0)
				charSets = charSets.Concat([CharacterSet.Letters]);
			if (numbers > 0)
				charSets = charSets.Concat([CharacterSet.Numbers]);
			if (symbols > 0)
				charSets = charSets.Concat([CharacterSet.Symbols]);

			// Avoid numbers and special characters at the beginning of the password but if there are no letters available, they will be used.
			// First symbols and then numbers will be avoided if possible.
			if (position == 1 && charSets.Count() > 1)
				charSets = charSets.Where(cs => cs != CharacterSet.Symbols);
			if (position == 1 && charSets.Count() > 1)
				charSets = charSets.Where(cs => cs != CharacterSet.Numbers);

			// Get a random character set from the available ones
			return charSets.ToArray()[_random.Next(charSets.Count())];
		}

		private char[] GetAndDiscountAvailableCharacters(CharacterSet characterSet, ref int letters, ref int numbers, ref int specialCharacters)
		{
			switch (characterSet)
			{
				case CharacterSet.Letters:
					letters--;
					return _letters;
				case CharacterSet.Numbers:
					numbers--;
					return _numbers;
				case CharacterSet.Symbols:
					specialCharacters--;
					return GetAvailableSymbols().ToArray();
				default:
					throw new NotImplementedException($"Character set {characterSet} is not implemented.");
			}
		}

		private char[] FilterPossibleCharacters(char[] aC, ref HandSide currentHand, Finger? currentFinger)
		{
			// CS1628 Cannot use ref [...] parameter 'currentHand' inside [...] a lambda expression [...].
			HandSide curHand = currentHand;

			var possibleChars = aC
				.Where(tn => !RemoveHighAsciiCharacters || tn < 128)
				.Where(tn => IsProperHand(tn, curHand))
				.Where(tn => IsProperFinger(tn, curHand, currentFinger));

			char[] @return = possibleChars.ToArray();
			if (@return.Length == 0)
				throw new InvalidOperationException($"There are no more characters available for the current hand ({currentHand}) and finger ({currentFinger}):" +
					$" '{string.Join(",", aC)}')");

			currentHand = ChooseHand(curHand);

			return @return;
		}

		protected override HandSide ChooseHand(HandSide currentHand)
		{
			if (KeystrokeOrder == KeystrokeOrder.Random)
				currentHand = HandSide.Any;
			else if (KeystrokeOrder == KeystrokeOrder.AlternatingStroke)
				currentHand = currentHand == HandSide.Left ? HandSide.Right : HandSide.Left;
			return currentHand;
		}

		private char ChooseCharacter(char[] charList, ref Finger? currentFinger)
		{
			if (charList.Length == 0)
				throw new InvalidOperationException("There are no more characters available.");

			char @return = charList[_random.Next(charList.Length)];

			currentFinger = GetFingerForKeystroke(@return.ToString());
			return @return;
		}
	}
}
