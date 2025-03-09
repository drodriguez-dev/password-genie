namespace PG.Logic.Passwords.Generators.Entities
{
	public class PasswordResult
	{
		public required string Password { get; set; }

		/// <summary>
		/// The entropy of the password based on the number of options on each decision point.
		/// </summary>
		public double TrueEntropy { get; set; }

		public PasswordStrength TrueStrength { get; set; }

		/// <summary>
		/// The entropy of the password based on the count of characters for each type and the pool of characters on the generated password.
		/// </summary>
		/// <remarks>
		/// For each common symbol type (lower case letters, upper case letters, numbers, etc.), it counts how many characters of that type there are in your password. It returns the number of bits of entropy is the password.
		/// </remarks>
		public double DerivedEntropy { get; set; }
		public PasswordStrength DerivedStrength { get; set; }
	}
}
