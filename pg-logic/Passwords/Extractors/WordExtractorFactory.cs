using PG.Entities.Files;
using PG.Logic.Common;
using PG.Logic.Passwords.Extractors.Entities;
using PG.Logic.Passwords.Loaders;

namespace PG.Logic.Passwords.Extractors
{
	public class WordExtractorFactory(IServiceProvider provider)
	{
		private readonly IServiceProvider _provider = provider;

		public IWordExtractor Create(DictionaryFormat format)
		{
			IDictionaryLoaderFactory loaderFactory = _provider.GetService(typeof(IDictionaryLoaderFactory)) as IDictionaryLoaderFactory
				?? throw new InvalidOperationException("Dictionary loader factory is not registered as a service provider.");

			IDictionaryLoader loader = format switch
			{
				DictionaryFormat.Plain => loaderFactory.CreateForDictionary(DictionaryType.PlainTextDictionary, Constants.DictionaryEncoding),
				_ => throw new InvalidOperationException($"Invalid generator type ('{format}')")
			};

			return new PlainTextWordExtractor(loader);
		}
	}
}
