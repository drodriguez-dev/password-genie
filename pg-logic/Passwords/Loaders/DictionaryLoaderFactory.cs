using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.Dictionaries;
using PG.Data.Files.DataFiles.WordTrees;
using PG.Entities.Files;
using System.Text;

namespace PG.Logic.Passwords.Loaders
{
	public class DictionaryLoaderFactory(IServiceProvider provider) : IDictionaryLoaderFactory
	{
		private readonly IServiceProvider _provider = provider;

		public IDictionaryLoader CreateForDictionary(DictionaryType type, Encoding encoding)
		{
			IDictionariesDataFactory dataFactory = _provider.GetService(typeof(IDictionariesDataFactory)) as IDictionariesDataFactory
				?? throw new InvalidOperationException("Dictionary data factory is not registered as a service provider.");

			IDictionariesData data = dataFactory.CreateForDictionaryFile(type, encoding);

			return new WordDictionaryLoader(data);
		}

		public IWordTreeData CreateForWordTree(DictionaryType type)
		{
			IDictionariesDataFactory dataFactory = _provider.GetService(typeof(IDictionariesDataFactory)) as IDictionariesDataFactory
				?? throw new InvalidOperationException("Dictionary data factory is not registered as a service provider.");

			return dataFactory.CreateForWordTreeFile(type);
		}
	}
}
