using PG.Interface.Command.PasswordGeneration.Entities;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.ErrorHandling;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Reflection;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Interface.Command.PasswordGeneration
{
	public class PassGenieParser(IServiceProvider provider)
	{
		private readonly IServiceProvider _provider = provider;

		public async Task<int> Parse(string[] arguments)
		{
			RootCommand rootCommand = CreateRootCommand();
			rootCommand.AddCommand(CreateGenerationCommand());

			Parser parser = new CommandLineBuilder(rootCommand)
				.UseVersionOption("--Version", "-v")
				.UseHelp()
				.UseEnvironmentVariableDirective()
				.UseParseDirective()
				.RegisterWithDotnetSuggest()
				.UseTypoCorrections()
				.UseParseErrorReporting()
				.CancelOnProcessTermination()
				.UseExceptionHandler(HandleCommandException)
				.Build();

			return await parser.InvokeAsync(arguments);
		}

		private static RootCommand CreateRootCommand()
		{
			var command = new RootCommand("Generates passwords based on different strategies and options");
			command.AddGlobalOption(new Option<int>(["--NumberOfPasswords", "-p"], () => 1, "Number of passwords to generate"));
			command.AddGlobalOption(new Option<int>(["--Length", "-l"], () => 12, "Length of the password (approximate)"));
			command.AddGlobalOption(new Option<int>(["--NumberOfNumbers", "-n"], () => 1, "Number of numbers in the password"));
			command.AddGlobalOption(new Option<int>(["--NumberOfSpecialCharacters", "-s"], () => 1, "Number of special characters in the password"));
			command.AddGlobalOption(new Option<bool>(["--IncludeGroupSymbols", "-sg"], () => true, @"Include group symbols ('()[]{}<>') in the password."));
			command.AddGlobalOption(new Option<bool>(["--IncludeMarkSymbols", "-sm"], () => true, @"Include mark symbols ('!@#$%^*+=|;:\""?') in the password."));
			command.AddGlobalOption(new Option<bool>(["--IncludeSeparatorSymbols", "-sp"], () => true, @"Include separator symbols (' -_/\&,.') in the password."));
			command.AddGlobalOption(new Option<string>(["--CustomSymbols", "-sc"], () => string.Empty, "Use custom set of symbols. All 'Include' options are ignored."));
			command.AddGlobalOption(new Option<bool>(["--RemoveHighAsciiTable", "-r"], () => false, @"Remove characters of the high ASCII table (128-255) from the password."));

			return command;
		}

		private System.CommandLine.Command CreateGenerationCommand()
		{
			var command = new System.CommandLine.Command("generate", "Generates passwords based on different strategies and options");
			command.AddArgument(new Argument<GeneratorType>("type", "The strategy to use for generating the password"));

			// Options for the random strategy
			command.AddOption(new Option<int>(["--NumberOfLetters", "-c"], () => 10, "Number of letters in the password"));

			// Options for the dictionary strategy
			command.AddOption(new Option<FileInfo>(["--Dictionary", "-d"], "Dictionary file path to use for generating the password"));
			command.AddOption(new Option<int>(["--NumberOfWords", "-w"], () => 2, "Number of words for generating the password"));
			command.AddOption(new Option<int>(["--AverageWordLength", "-wl"], () => 6, "Average word length in the password"));
			command.AddOption(new Option<int>(["--DepthLevel", "-wd"], () => 3, "Depth level for the word generation"));

			command.Handler = CommandHandler.Create<GeneratorType, PassGenieSettings>(ExecuteCommand);

			return command;
		}

		private void ExecuteCommand(GeneratorType type, PassGenieSettings settings)
		{
			var factory = _provider.GetService(typeof(PasswordGeneratorFactory)) as PasswordGeneratorFactory
				?? throw new InvalidOperationException("Password generator factory is not registered as a service provider.");

			IPasswordGenerator generator = type switch
			{
				GeneratorType.Random => factory.Create(type, ConvertToRandomGeneratorOptions(settings)),
				GeneratorType.Dictionary => factory.Create(type, ConvertToDictionaryGeneratorOptions(settings)),
				_ => throw new InvalidOperationException($"Invalid generator type ('{type}')")
			};

			Console.WriteLine(generator.Generate());
		}

		private static RandomPasswordGeneratorOptions ConvertToRandomGeneratorOptions(PassGenieSettings settings)
		{
			return new()
			{
				NumberOfPasswords = settings.NumberOfPasswords,
				MinimumLength = settings.Length,
				NumberOfLetters = settings.NumberOfLetters,
				NumberOfNumbers = settings.NumberOfNumbers,
				NumberOfSpecialCharacters = settings.NumberOfSpecialCharacters,
				IncludeSetSymbols = settings.IncludeGroupSymbols,
				IncludeMarkSymbols = settings.IncludeMarkSymbols,
				IncludeSeparatorSymbols = settings.IncludeSeparatorSymbols,
				CustomSpecialCharacters = settings.CustomSymbols.ToCharArray(),
				RemoveHighAsciiCharacters = settings.RemoveHighAsciiTable,
			};
		}

		private static DictionaryPasswordGeneratorOptions ConvertToDictionaryGeneratorOptions(PassGenieSettings settings)
		{
			if (settings.Dictionary is null)
				throw new InvalidDictionaryException("Dictionary file path is required for the dictionary strategy.");

			if (!settings.Dictionary.Exists)
				throw new InvalidDictionaryException($"Dictionary file '{settings.Dictionary.FullName}' does not exist.");

			return new()
			{
				NumberOfPasswords = settings.NumberOfPasswords,
				MinimumLength = settings.Length,
				NumberOfNumbers = settings.NumberOfNumbers,
				NumberOfSpecialCharacters = settings.NumberOfSpecialCharacters,
				IncludeSetSymbols = settings.IncludeGroupSymbols,
				IncludeMarkSymbols = settings.IncludeMarkSymbols,
				IncludeSeparatorSymbols = settings.IncludeSeparatorSymbols,
				CustomSpecialCharacters = settings.CustomSymbols.ToCharArray(),
				RemoveHighAsciiCharacters = settings.RemoveHighAsciiTable,
				File = settings.Dictionary.FullName,
				NumberOfWords = settings.NumberOfWords,
				AverageWordLength = settings.AverageWordLength,
				DepthLevel = settings.DepthLevel,
			};
		}

		private void HandleCommandException(Exception exception, InvocationContext context)
		{
			if (exception is TargetInvocationException targetInvocationException)
				exception = targetInvocationException.InnerException ?? exception;

			if (exception is BaseException baseException)
			{
				WriteConsole(ConsoleColor.Yellow, baseException.Message);
				context.ExitCode = 1;
			}
			else
			{
				WriteConsole(ConsoleColor.Red, exception.Message);
				context.ExitCode = int.MaxValue;
			}
		}

		private static void WriteConsole(ConsoleColor color, string message)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message);
			Console.ResetColor();
		}
	}
}