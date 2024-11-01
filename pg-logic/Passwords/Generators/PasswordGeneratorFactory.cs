using PG.Entities.Files;
using PG.Entities.WordTrees;
using PG.Logic.Common;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loaders;
using PG.Shared.Services;

namespace PG.Logic.Passwords.Generators
{
	public class PasswordGeneratorFactory(IServiceProvider provider)
	{
		private readonly IServiceProvider _provider = provider;

		public IPasswordGenerator Create(GeneratorType type, CommonPasswordGeneratorOptions options)
		{
			return type switch
			{
				GeneratorType.Random => CreateRandomPasswordGenerator(options),
				GeneratorType.Dictionary => CreateDictionaryBasedPasswordGenerator(options),
				_ => throw new NotImplementedException($"Generator type {type} is not implemented."),
			};
		}

		private RandomPasswordGenerator CreateRandomPasswordGenerator(CommonPasswordGeneratorOptions options)
		{
			var randomOptions = options as RandomPasswordGeneratorOptions
				?? throw new ArgumentException($"Options must be of type {nameof(RandomPasswordGeneratorOptions)}.");

			var random = _provider.GetService(typeof(RandomService)) as RandomService
				?? throw new InvalidOperationException("Random service is not registered as a service provider.");

			return new RandomPasswordGenerator(randomOptions, random);
		}

		private DictionaryPasswordGenerator CreateDictionaryBasedPasswordGenerator(CommonPasswordGeneratorOptions options)
		{
			var dictionaryOptions = options as DictionaryPasswordGeneratorOptions
				?? throw new ArgumentException($"Options must be of type {nameof(DictionaryPasswordGeneratorOptions)}.");

			RandomService random = _provider.GetService(typeof(RandomService)) as RandomService
				?? throw new InvalidOperationException("Random service is not registered as a service provider.");

			IDictionaryLoaderFactory loaderFactory = _provider.GetService(typeof(IDictionaryLoaderFactory)) as IDictionaryLoaderFactory
				?? throw new InvalidOperationException("Dictionary loader factory is not registered as a service provider.");

			WordDictionaryTree wordTree = dictionaryOptions.Type switch
			{
				DictionaryType.PlainTextDictionary => loaderFactory.CreateForDictionary(dictionaryOptions.Type, Constants.DictionaryEncoding).Load(dictionaryOptions.File),
				DictionaryType.WordTree => loaderFactory.CreateForWordTree(dictionaryOptions.Type).FetchTree(dictionaryOptions.File),
				_ => throw new NotSupportedException($"Dictionary type '{dictionaryOptions.Type}' is not implemented."),
			};

			return new DictionaryPasswordGenerator(dictionaryOptions, random, wordTree);
		}
	}
}
