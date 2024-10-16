using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.Dictionaries;
using PG.Data.Files.DataFiles.WordTrees;
using System.Text;

namespace PG.Logic.Passwords.Loader
{
	public class DictionaryLoaderFactory(IServiceProvider provider) : IDictionaryLoaderFactory
	{
		private readonly IServiceProvider _provider = provider;

		public IDictionaryLoader CreateForDictionary(string filePath, Encoding encoding)
		{
			IDictionariesDataFactory dataFactory = _provider.GetService(typeof(IDictionariesDataFactory)) as IDictionariesDataFactory
				?? throw new InvalidOperationException("Dictionary data factory is not registered as a service provider.");

			IDictionariesData data = dataFactory.CreateForDictionaryFile(filePath, encoding);

			return new WordDictionaryLoader(data);
		}

		public IWordTreeData CreateForWordTree(string filePath)
		{
			IDictionariesDataFactory dataFactory = _provider.GetService(typeof(IDictionariesDataFactory)) as IDictionariesDataFactory
				?? throw new InvalidOperationException("Dictionary data factory is not registered as a service provider.");

			return dataFactory.CreateForWordTreeFile(filePath);
		}
	}
}
