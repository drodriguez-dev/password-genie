using PG.Entities.Files;
using PG.Interface.Command.PasswordGeneration.Entities;
using PG.Logic.Passwords.Extractors;
using PG.Logic.Passwords.Extractors.Entities;
using PG.Logic.Passwords.Generators;
using PG.Logic.Passwords.Generators.Entities;
using PG.Shared.ErrorHandling;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Reflection;
using static PG.Logic.ErrorHandling.BusinessExceptions;

namespace PG.Interface.Command.PasswordGeneration
{
	public class PassGenieParser(IServiceProvider provider)
	{
		private readonly IServiceProvider _provider = provider;

		// Return values
		private const int NO_ERROR = 0;
		private const int UNEXPECTED_ERROR = int.MaxValue;

		public event Action<HumanReadableMessage>? OutputReport;

		public async Task<int> ParseAndExecute(string[] arguments)
		{
			RootCommand rootCommand = CreateRootCommand();
			rootCommand.AddCommand(CreateGenerationCommand());
			rootCommand.AddCommand(CreateExtractionCommand());

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
			command.AddGlobalOption(new Option<bool>(["--Verbose"], "Show additional information about the generated password."));

			return command;
		}

		private System.CommandLine.Command CreateGenerationCommand()
		{
			var command = new System.CommandLine.Command("generate", "Generates passwords based on different strategies and options");
			command.AddArgument(new Argument<GeneratorType>("type", "The strategy to use for generating the password"));

			// Options for both of the strategies
			command.AddOption(new Option<int>(["--NumberOfPasswords", "-p"], () => 1, "Number of passwords to generate"));
			command.AddOption(new Option<int>(["--NumberOfNumbers", "-n"], () => 1, "Number of numbers in the password"));
			command.AddOption(new Option<int>(["--NumberOfSpecialCharacters", "-s"], () => 1, "Number of special characters in the password"));
			command.AddOption(new Option<bool>(["--IncludeGroupSymbols", "-sg"], () => true, @"Include group symbols ('()[]{}<>') in the password."));
			command.AddOption(new Option<bool>(["--IncludeMarkSymbols", "-sm"], () => true, @"Include mark symbols ('!@#$%^*+=|;:\""?') in the password."));
			command.AddOption(new Option<bool>(["--IncludeSeparatorSymbols", "-sp"], () => true, @"Include separator symbols (' -_/\&,.') in the password."));
			command.AddOption(new Option<string>(["--CustomSymbols", "-sc"], () => string.Empty, "Use custom set of symbols. All 'Include' options are ignored."));
			command.AddOption(new Option<bool>(["--RemoveHighAsciiTable", "-r"], () => false, @"Remove characters of the high ASCII table (128-255) from the password."));

			// Options for the random strategy
			command.AddOption(new Option<int>(["--NumberOfLetters", "-c"], () => 10, "Number of letters in the password"));

			// Options for the dictionary strategy
			var dictionaryOption = new Option<FileInfo>(["--Dictionary", "-d"], "Dictionary file path to use for generating the password");
			var wordTreeOption = new Option<FileInfo>(["--WordTree", "-wt"], "Word tree file (.gz) path to use for generating the password");
			dictionaryOption.AddValidator(validateOneFileOnly);
			wordTreeOption.AddValidator(validateOneFileOnly);
			command.AddOption(dictionaryOption);
			command.AddOption(wordTreeOption);

			command.AddOption(new Option<int>(["--NumberOfWords", "-w"], () => 2, "Number of words for generating the password"));
			command.AddOption(new Option<int>(["--AverageWordLength", "-wl"], () => 6, "Average word length in the password"));
			command.AddOption(new Option<int>(["--DepthLevel", "-wd"], () => 3, "Depth level for the word generation"));
			command.AddOption(new Option<KeystrokeOrder>(["--KeystrokeOrder", "-ko"], () => KeystrokeOrder.Random,
				$"The order in which keystrokes are generated ({string.Join(", ", Enum.GetNames(typeof(KeystrokeOrder)))})"));

			command.Handler = CommandHandler.Create<GeneratorType, GeneratorSettings>(ExecuteGeneration);

			return command;

			// Validation to avoid using both dictionary and word tree options
			static void validateOneFileOnly(OptionResult results)
			{
				var dictionaryOption = results.Children.FirstOrDefault(o => o.Symbol.Name == "Dictionary");
				var wordTreeOption = results.Children.FirstOrDefault(o => o.Symbol.Name == "WordTree");

				if (dictionaryOption != null && wordTreeOption != null)
					results.ErrorMessage = "Only one of '--Dictionary' or '--WordTree' options can be used.";
			}
		}

		private System.CommandLine.Command CreateExtractionCommand()
		{
			var command = new System.CommandLine.Command("extract", "Generates passwords based on different strategies and options");
			command.AddArgument(new Argument<DictionaryFormat>("format", "The format of the dictionary"));

			command.AddOption(new Option<FileInfo>(["--Input", "-i"], "File path for the dictionary file"));
			command.AddOption(new Option<FileInfo>(["--Output", "-o"], "File path for the generated word tree file"));
			command.AddOption(new Option<bool>(["--Overwrite", "-ow"], () => false, "Overwrite the output file if it already exists"));

			command.Handler = CommandHandler.Create<DictionaryFormat, ExtractorSettings>(ExecuteExtraction);

			return command;
		}

		private int ExecuteGeneration(GeneratorType type, GeneratorSettings settings)
		{
			var factory = _provider.GetService(typeof(PasswordGeneratorFactory)) as PasswordGeneratorFactory
				?? throw new InvalidOperationException("Password generator factory is not registered as a service provider.");

			IPasswordGenerator generator = type switch
			{
				GeneratorType.Random => factory.Create(type, ConvertToRandomGeneratorOptions(settings)),
				GeneratorType.Dictionary => factory.Create(type, ConvertToDictionaryGeneratorOptions(settings)),
				_ => throw new InvalidOperationException($"Invalid generator type ('{type}')")
			};

			Output(TraceLevel.Verbose, "Generating {0} password(s) with the following settings:", settings.NumberOfPasswords);
			Output(TraceLevel.Verbose, "  Numbers: {0}", settings.NumberOfNumbers);
			Output(TraceLevel.Verbose, "  Special characters: {0}", settings.NumberOfSpecialCharacters);
			Output(TraceLevel.Verbose, "  Include group symbols: {0}", settings.IncludeGroupSymbols);
			Output(TraceLevel.Verbose, "  Include mark symbols: {0}", settings.IncludeMarkSymbols);
			Output(TraceLevel.Verbose, "  Include separator symbols: {0}", settings.IncludeSeparatorSymbols);
			Output(TraceLevel.Verbose, "  Custom symbols: {0}", settings.CustomSymbols);
			Output(TraceLevel.Verbose, "  Remove high ASCII table: {0}", settings.RemoveHighAsciiTable);

			var result = generator.Generate();
			foreach (var passwordResult in result.Passwords)
			{
				Output(TraceLevel.Info, "{0}", passwordResult.Password);

				if (settings.Verbose)
				{
					Output(TraceLevel.Verbose, "  True entropy: {0:N2} ({1})", passwordResult.TrueEntropy, GetEntropyText(passwordResult.TrueStrength));
					Output(TraceLevel.Verbose, "  Derived entropy: {0:N2} ({1})", passwordResult.DerivedEntropy, GetEntropyText(passwordResult.DerivedStrength));
				}
			}

			return NO_ERROR;
		}

		private static RandomPasswordGeneratorOptions ConvertToRandomGeneratorOptions(GeneratorSettings settings)
		{
			return new()
			{
				NumberOfPasswords = settings.NumberOfPasswords,
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

		private static DictionaryPasswordGeneratorOptions ConvertToDictionaryGeneratorOptions(GeneratorSettings settings)
		{
			FileInfo file = settings.Dictionary ?? settings.WordTree
				?? throw new InvalidDictionaryException("Dictionary or word tree file path is required for the dictionary strategy.");

			if (!file.Exists)
				throw new InvalidDictionaryException($"Data file '{file.FullName}' does not exist.");

			DictionaryType type = file.Extension.ToLower() switch
			{
				".txt" => DictionaryType.PlainTextDictionary,
				".gz" => DictionaryType.WordTree,
				_ => throw new NotSupportedException($"File extension '{file.Extension}' is not supported.")
			};

			byte[] fileBytes = File.ReadAllBytes(file.FullName);

			return new()
			{
				NumberOfPasswords = settings.NumberOfPasswords,
				NumberOfNumbers = settings.NumberOfNumbers,
				NumberOfSpecialCharacters = settings.NumberOfSpecialCharacters,
				IncludeSetSymbols = settings.IncludeGroupSymbols,
				IncludeMarkSymbols = settings.IncludeMarkSymbols,
				IncludeSeparatorSymbols = settings.IncludeSeparatorSymbols,
				CustomSpecialCharacters = settings.CustomSymbols.ToCharArray(),
				RemoveHighAsciiCharacters = settings.RemoveHighAsciiTable,
				Type = type,
				File = new MemoryStream(fileBytes),
				NumberOfWords = settings.NumberOfWords,
				AverageWordLength = settings.AverageWordLength,
				DepthLevel = settings.DepthLevel,
				KeystrokeOrder = settings.KeystrokeOrder,
			};
		}

		/// <summary>
		/// Using predefined ranges, returns a text representation of the entropy.
		/// </summary>
		private static string GetEntropyText(PasswordStrength strength)
		{
			return strength switch
			{
				PasswordStrength.VeryWeak => "Very weak",
				PasswordStrength.Weak => "Weak",
				PasswordStrength.Reasonable => "Reasonable",
				PasswordStrength.Strong => "Strong",
				PasswordStrength.VeryStrong => "Very strong",
				_ => "(error)"
			};
		}

		private int ExecuteExtraction(DictionaryFormat format, ExtractorSettings settings)
		{
			FileInfo inputFile = settings.Input
				?? throw new InvalidDictionaryException("Input file is required to extract the words.");

			if (!inputFile.Exists)
				throw new InvalidDictionaryException($"Data file '{inputFile.FullName}' does not exist.");

			FileInfo outputFile = settings.Output
				?? throw new InvalidDictionaryException("Ouput file is required to save extracted words.");

			if (outputFile.Exists && !settings.Overwrite)
				throw new InvalidDictionaryException("Output file already exists, overwrite option is not enabled");

			var factory = _provider.GetService(typeof(WordExtractorFactory)) as WordExtractorFactory
				?? throw new InvalidOperationException("Word extractor factory is not registered as a service provider.");

			IWordExtractor extractor = factory.Create(format);

			using (Stream inputStream = new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (Stream outputStream = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.Read))
				extractor.ExtractWordTree(inputStream, outputStream);

			return NO_ERROR;
		}

		private void HandleCommandException(Exception exception, InvocationContext context)
		{
			if (exception is TargetInvocationException targetInvocationException)
				exception = targetInvocationException.InnerException ?? exception;

			if (exception is BaseException baseException)
				Output(TraceLevel.Warning, "{0}", baseException.Message);
			else
				Output(TraceLevel.Error, "{0}", exception.Message);

			context.ExitCode = UNEXPECTED_ERROR;
		}

		private void Output(TraceLevel level, string format, params object[] args)
		{
			OutputReport?.Invoke(new HumanReadableMessage(level, format, args));
		}
	}
}