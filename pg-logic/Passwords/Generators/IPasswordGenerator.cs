using PG.Logic.Passwords.Generators.Entities;

namespace PG.Logic.Passwords.Generators
{
	public interface IPasswordGenerator
	{
		/// <summary>
		/// Configure the generator with the given options after the generator is created.
		/// </summary>
		/// <param name="config">The class that contains the configuration options for the generator is specific to each generator.</param>
		void Configure(CommonPasswordGeneratorOptions config);

		/// <summary>
		/// Generate passwords based on the generator's rules.
		/// </summary>
		/// <returns>Text with the generated passwords separated by a new line.</returns>
		GenerationResult Generate();
	}
}
