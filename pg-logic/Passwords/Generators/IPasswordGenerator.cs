namespace PG.Logic.Passwords.Generators
{
	public interface IPasswordGenerator
	{
		/// <summary>
		/// Generate passwords based on the generator's rules.
		/// </summary>
		/// <returns>Text with the generated passwords separated by a new line.</returns>
		string Generate();

		double GetAndResetPasswordEntropy();
	}
}
