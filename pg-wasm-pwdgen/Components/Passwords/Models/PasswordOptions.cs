using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PG.Wasm.PasswordGenerator.Components.Passwords.Models
{
	public class PasswordOptions : INotifyPropertyChanged
	{
		private int numberOfLetters = 8;
		private int numberOfNumbers = 1;
		private int numberOfSpecialCharacters = 1;

		[Required(ErrorMessage = "Number of letters is required")]
		[Range(0, 100, ErrorMessage = "Number of letters should be between 0 and 100.")]
		public int NumberOfLetters { get => numberOfLetters; set { numberOfLetters = value; OnPropertyChanged(nameof(NumberOfLetters)); } }

		[Required(ErrorMessage = "Number of numbers is required")]
		[Range(0, 100, ErrorMessage = "Number of numbers should be between 0 and 100.")]
		public int NumberOfNumbers { get => numberOfNumbers; set { numberOfNumbers = value; OnPropertyChanged(nameof(NumberOfNumbers)); } }

		[Required(ErrorMessage = "Number of symbols is required")]
		[Range(0, 100, ErrorMessage = "Number of symbols should be between 0 and 100.")]
		public int NumberOfSpecialCharacters { get => numberOfSpecialCharacters; set { numberOfSpecialCharacters = value; OnPropertyChanged(nameof(NumberOfSpecialCharacters)); } }

		public bool HasChanges { get; set; } = false;

		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged(string propertyName)
		{
			HasChanges = true;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
