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
		private bool isRandomStrokeOrder = false;
		private bool isAlternatingStrokeOrder = true;
		private bool isAlternatingWordKeystroke = false;
		private bool isOnlyLeftKeystrokes = false;
		private bool isOnlyRightKeystrokes = false;
		private int numberOfWords = 2;
		private int averageWordLength = 7;
		private int depthLevel = 4;
		private int minimumLength = 1;

		#region Dictionary specific options
		[Required(ErrorMessage = "Number of words is required")]
		[Range(1, 30, ErrorMessage = "Number of words should be between 1 and 30.")]
		public int NumberOfWords { get => numberOfWords; set { numberOfWords = value; OnPropertyChanged(nameof(NumberOfWords)); } }

		[Required(ErrorMessage = "Average word length is required")]
		[Range(4, 20, ErrorMessage = "Average word length should be between 4 and 20.")]
		public int AverageWordLength { get => averageWordLength; set { averageWordLength = value; OnPropertyChanged(nameof(AverageWordLength)); } }

		// Depth level must be lower than the average word length
		[Required(ErrorMessage = "Depth level is required")]
		[Range(2, 20, ErrorMessage = "Depth level should be between 2 and 20.")]
		[CustomValidation(typeof(PasswordOptions), nameof(ValidateDepthLowerThanAverage), ErrorMessage = "Depth level must be lower than the average word length")]
		public int DepthLevel { get => depthLevel; set { depthLevel = value; OnPropertyChanged(nameof(DepthLevel)); } }
		#endregion

		#region Random specific options
		[Required(ErrorMessage = "Number of letters is required")]
		[Range(0, 100, ErrorMessage = "Number of letters should be between 0 and 100.")]
		public int NumberOfLetters { get => numberOfLetters; set { numberOfLetters = value; OnPropertyChanged(nameof(NumberOfLetters)); } }
		#endregion

		#region Common options
		[Required(ErrorMessage = "Number of numbers is required")]
		[Range(0, 100, ErrorMessage = "Number of numbers should be between 0 and 100.")]
		public int NumberOfNumbers { get => numberOfNumbers; set { numberOfNumbers = value; OnPropertyChanged(nameof(NumberOfNumbers)); } }

		[Required(ErrorMessage = "Number of symbols is required")]
		[Range(0, 100, ErrorMessage = "Number of symbols should be between 0 and 100.")]
		public int NumberOfSpecialCharacters { get => numberOfSpecialCharacters; set { numberOfSpecialCharacters = value; OnPropertyChanged(nameof(NumberOfSpecialCharacters)); } }

		public bool IncludeSetSymbols { get => includeSetSymbols; set { includeSetSymbols = value; OnPropertyChanged(nameof(IncludeSetSymbols)); } }

		public bool IncludeSeparatorSymbols { get => includeSeparatorSymbols; set { includeSeparatorSymbols = value; OnPropertyChanged(nameof(IncludeSeparatorSymbols)); } }

		public bool IncludeMarkSymbols { get => includeMarkSymbols; set { includeMarkSymbols = value; OnPropertyChanged(nameof(IncludeMarkSymbols)); } }

		public string CustomSpecialCharacters { get => customSpecialCharacters; set { customSpecialCharacters = value; OnPropertyChanged(nameof(CustomSpecialCharacters)); } }

		[Required(ErrorMessage = "Minimum length is required")]
		[Range(1, 20, ErrorMessage = "Minimum length should be between 1 and 20.")]
		public int MinimumLength { get => minimumLength; set { minimumLength = value; OnPropertyChanged(nameof(MinimumLength)); } }

		public bool IsRandomStrokeOrder { get => isRandomStrokeOrder; set { isRandomStrokeOrder = value; OnPropertyChanged(nameof(IsRandomStrokeOrder)); } }

		public bool IsAlternatingStrokeOrder { get => isAlternatingStrokeOrder; set { isAlternatingStrokeOrder = value; OnPropertyChanged(nameof(IsAlternatingStrokeOrder)); } }

		public bool IsAlternatingWordKeystroke { get => isAlternatingWordKeystroke; set { isAlternatingWordKeystroke = value; OnPropertyChanged(nameof(IsAlternatingWordKeystroke)); } }

		public bool IsOnlyLeftKeystrokes { get => isOnlyLeftKeystrokes; set { isOnlyLeftKeystrokes = value; OnPropertyChanged(nameof(IsOnlyLeftKeystrokes)); } }

		public bool IsOnlyRightKeystrokes { get => isOnlyRightKeystrokes; set { isOnlyRightKeystrokes = value; OnPropertyChanged(nameof(IsOnlyRightKeystrokes)); } }

		public KeystrokeOrder KeystrokeOrder
		{
			get
			{
				if (IsRandomStrokeOrder) return KeystrokeOrder.Random;
				if (IsAlternatingStrokeOrder) return KeystrokeOrder.AlternatingStroke;
				if (IsAlternatingWordKeystroke) return KeystrokeOrder.AlternatingWord;
				if (IsOnlyLeftKeystrokes) return KeystrokeOrder.OnlyLeft;
				if (IsOnlyRightKeystrokes) return KeystrokeOrder.OnlyRight;
				return KeystrokeOrder.Random;
			}

			set
			{
				IsRandomStrokeOrder = value == KeystrokeOrder.Random;
				IsAlternatingStrokeOrder = value == KeystrokeOrder.AlternatingStroke;
				IsAlternatingWordKeystroke = value == KeystrokeOrder.AlternatingWord;
				IsOnlyLeftKeystrokes = value == KeystrokeOrder.OnlyLeft;
				IsOnlyRightKeystrokes = value == KeystrokeOrder.OnlyRight;
			}
		}
		#endregion

		public bool HasChanges { get; set; } = false;

		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			HasChanges = true;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static ValidationResult ValidateDepthLowerThanAverage(object _, ValidationContext validationContext)
		{
			var options = validationContext.ObjectInstance as PasswordOptions;
			if (options is not null && options.DepthLevel < options.AverageWordLength)
				return ValidationResult.Success!;

			return new ValidationResult("Depth level must be lower than the average word length");
		}
	}
}
