using PG.Logic.Passwords.Generators.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PG.Wasm.PasswordGenerator.Components.Passwords.Models
{
	public class PasswordOptions : INotifyPropertyChanged
	{
		private int numberOfLetters = 8;
		private int numberOfNumbers = 1;
		private int numberOfSpecialCharacters = 1;
		private bool includeSetSymbols = true;
		private bool includeSeparatorSymbols = true;
		private bool includeMarkSymbols = true;
		private string customSpecialCharacters = string.Empty;
		private bool isRandomStrokeOrder = true;
		private bool isAlternatingStrokeOrder = false;
		private bool isAlternatingWordKeystroke = false;
		private bool isOnlyLeftKeystrokes = false;
		private bool isOnlyRightKeystrokes = false;

		[Required(ErrorMessage = "Number of letters is required")]
		[Range(0, 100, ErrorMessage = "Number of letters should be between 0 and 100.")]
		public int NumberOfLetters { get => numberOfLetters; set { numberOfLetters = value; OnPropertyChanged(nameof(NumberOfLetters)); } }

		[Required(ErrorMessage = "Number of numbers is required")]
		[Range(0, 100, ErrorMessage = "Number of numbers should be between 0 and 100.")]
		public int NumberOfNumbers { get => numberOfNumbers; set { numberOfNumbers = value; OnPropertyChanged(nameof(NumberOfNumbers)); } }

		[Required(ErrorMessage = "Number of symbols is required")]
		[Range(0, 100, ErrorMessage = "Number of symbols should be between 0 and 100.")]
		public int NumberOfSpecialCharacters { get => numberOfSpecialCharacters; set { numberOfSpecialCharacters = value; OnPropertyChanged(nameof(NumberOfSpecialCharacters)); } }

		public bool IncludeSetSymbols { get => includeSetSymbols; set { includeSetSymbols = value; OnPropertyChanged(nameof(IncludeSetSymbols)); } }

		public bool IncludeSeparatorSymbols { get => includeSeparatorSymbols; set { includeSeparatorSymbols = value; OnPropertyChanged(nameof(IncludeSeparatorSymbols)); } }

		public bool IncludeMarkSymbols { get => includeMarkSymbols; set { includeMarkSymbols = value; OnPropertyChanged(nameof(IncludeMarkSymbols)); } }

		public string CustomSpecialCharacters { get => customSpecialCharacters; set { customSpecialCharacters = value; OnPropertyChanged(nameof(IncludeMarkSymbols)); } }

		public bool IsRandomStrokeOrder { get => isRandomStrokeOrder; set { isRandomStrokeOrder = value; OnPropertyChanged(nameof(IsRandomStrokeOrder)); } }

		public bool IsAlternatingStrokeOrder { get => isAlternatingStrokeOrder; set { isAlternatingStrokeOrder = value; OnPropertyChanged(nameof(IsAlternatingStrokeOrder)); } }

		public bool IsAlternatingWordKeystroke { get => isAlternatingWordKeystroke; set { isAlternatingWordKeystroke = value; OnPropertyChanged(nameof(IsAlternatingWordKeystroke)); } }

		public bool IsOnlyLeftKeystrokes { get => isOnlyLeftKeystrokes; set { isOnlyLeftKeystrokes = value; OnPropertyChanged(nameof(IsOnlyLeftKeystrokes)); } }

		public bool IsOnlyRightKeystrokes { get => isOnlyRightKeystrokes; set { isOnlyRightKeystrokes = value; OnPropertyChanged(nameof(IsOnlyRightKeystrokes)); } }

		public KeystrokeOrder KeystrokeOrder
		{
			get {
				if (IsRandomStrokeOrder) return KeystrokeOrder.Random;
				if (IsAlternatingStrokeOrder) return KeystrokeOrder.AlternatingStroke;
				if (IsAlternatingWordKeystroke) return KeystrokeOrder.AlternatingWord;
				if (IsOnlyLeftKeystrokes) return KeystrokeOrder.OnlyLeft;
				if (IsOnlyRightKeystrokes) return KeystrokeOrder.OnlyRight;
				return KeystrokeOrder.Random;
			}

			set {
				IsRandomStrokeOrder = value == KeystrokeOrder.Random;
				IsAlternatingStrokeOrder = value == KeystrokeOrder.AlternatingStroke;
				IsAlternatingWordKeystroke = value == KeystrokeOrder.AlternatingWord;
				IsOnlyLeftKeystrokes = value == KeystrokeOrder.OnlyLeft;
				IsOnlyRightKeystrokes = value == KeystrokeOrder.OnlyRight;
			}
		}

		public bool HasChanges { get; set; } = false;

		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			HasChanges = true;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
