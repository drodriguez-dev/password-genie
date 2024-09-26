using Microsoft.Extensions.DependencyInjection;
using PG.Data.Files.Dictionaries;
using PG.Interface.Command.PasswordGeneration;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Loader;

namespace PG.Console.PasswordGenie
{
	static class Program
	{
		private static readonly ServiceProvider _provider = GetServiceProvider();

		static int Main(string[] args)
		{
			PassGenieParser passGenieParser = _provider.GetRequiredService<PassGenieParser>();
			return passGenieParser.ParseAndExecute(args).Result;
		}

		private static ServiceProvider GetServiceProvider()
		{
			return new ServiceCollection()
				.AddSingleton<PassGenieParser>()
				.AddSingleton<PasswordGeneratorFactory>()
				.AddSingleton<IDictionaryLoaderFactory, DictionaryLoaderFactory>()
				.AddSingleton<IDictionariesDataFactory, DictionariesDataFactory>()
				.BuildServiceProvider();
		}
	}
}