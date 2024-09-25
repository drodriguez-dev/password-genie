using PG.Logic.Common;
using PG.Logic.Passwords.Generators.Entities;
using PG.Logic.Passwords.Loader;

namespace PG.Logic.Passwords.Generators
{
	public class PasswordGeneratorFactory(IServiceProvider provider)
	{
		private readonly IServiceProvider _provider = provider;

		public IPasswordGenerator Create(GeneratorType type, CommonPasswordGeneratorOptions options)
		{
			switch (type)
			{
				case GeneratorType.Random:
					var randomOptions = options as RandomPasswordGeneratorOptions
						?? throw new ArgumentException($"Options must be of type {nameof(RandomPasswordGeneratorOptions)}.");

					return new RandomPasswordGenerator(randomOptions);
				case GeneratorType.Dictionary:
					var dictionaryOptions = options as DictionaryPasswordGeneratorOptions
						?? throw new ArgumentException($"Options must be of type {nameof(DictionaryPasswordGeneratorOptions)}.");

					IDictionaryLoaderFactory loaderFactory = _provider.GetService(typeof(IDictionaryLoaderFactory)) as IDictionaryLoaderFactory
						?? throw new InvalidOperationException("Dictionary loader factory is not registered as a service provider.");

					var loader = loaderFactory.Create(dictionaryOptions.File, Constants.DictionaryEncoding);

					return new DictionaryPasswordGenerator(dictionaryOptions, loader);
				default:
					throw new NotImplementedException($"Generator type {type} is not implemented.");
			}
		}
	}
}
