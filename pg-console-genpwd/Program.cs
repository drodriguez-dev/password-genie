using Microsoft.Extensions.DependencyInjection;
using PG.Data.Files.Dictionaries;
using PG.Interface.Command.PasswordGeneration;
using PG.Interface.Command.PasswordGeneration.Entities;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Loader;
using PG.Shared.Services;

namespace PG.Console.PasswordGenie
{
	static class Program
	{
		private static readonly ServiceProvider _provider = GetServiceProvider();

		static async Task<int> Main(string[] args)
		{
			PassGenieParser passGenieParser = _provider.GetRequiredService<PassGenieParser>();
			passGenieParser.OutputReport += HandleOutputReport;
			int commandExecution = await passGenieParser.ParseAndExecute(args);

			return commandExecution;
		}

		/// <summary>
		/// Handles the output report by writing to the console or error stream with the appropriate color.
		/// </summary>
		private static void HandleOutputReport(HumanReadableMessage message)
		{
			switch (message.Type)
			{
				case System.Diagnostics.TraceLevel.Error:
					System.Console.ForegroundColor = ConsoleColor.Red;
					System.Console.Error.WriteLine(message.Format, message.Args);
					break;
				case System.Diagnostics.TraceLevel.Warning:
					System.Console.ForegroundColor = ConsoleColor.Yellow;
					System.Console.Error.WriteLine(message.Format, message.Args);
					break;
				case System.Diagnostics.TraceLevel.Info:
					System.Console.ForegroundColor = ConsoleColor.White;
					System.Console.WriteLine(message.Format, message.Args);
					break;
				case System.Diagnostics.TraceLevel.Verbose:
					System.Console.ForegroundColor = ConsoleColor.DarkGray;
					System.Console.WriteLine(message.Format, message.Args);
					break;
				case System.Diagnostics.TraceLevel.Off:
				default:
					break;
			}

			System.Console.ResetColor();
		}

		private static ServiceProvider GetServiceProvider()
		{
			return new ServiceCollection()
				.AddSingleton<PassGenieParser>()
				.AddSingleton<PasswordGeneratorFactory>()
				.AddSingleton<IDictionaryLoaderFactory, DictionaryLoaderFactory>()
				.AddSingleton<IDictionariesDataFactory, DictionariesDataFactory>()
				.AddTransient<RandomService>()
				.BuildServiceProvider();
		}
	}
}