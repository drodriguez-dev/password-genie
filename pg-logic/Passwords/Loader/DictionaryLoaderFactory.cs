using PG.Data.Files.Dictionaries;
using System.Text;

namespace PG.Logic.Passwords.Loader
{
	public class DictionaryLoaderFactory(IServiceProvider provider) : IDictionaryLoaderFactory
	{
		private readonly IServiceProvider _provider = provider;

		public IDictionaryLoader Create(string filePath, Encoding encoding)
		{
			IDictionariesDataFactory dataFactory = _provider.GetService(typeof(IDictionariesDataFactory)) as IDictionariesDataFactory
				?? throw new InvalidOperationException("Dictionary data factory is not registered as a service provider.");

			IDictionariesData data = dataFactory.CreateForFile(filePath, encoding);

			return new WordDictionaryLoader(data);
		}
	}
}
