using PG.Data.Files.DataFiles.WordTrees;
using PG.Entities.WordTrees;
using PG.Logic.Passwords.Loaders;

namespace PG.Logic.Passwords.Extractors
{
	public class PlainTextWordExtractor(IDictionaryLoader loader) : IWordExtractor
	{
		private readonly IDictionaryLoader _loader = loader;

		public void ExtractWordTree(Stream inputStream, Stream outputStream)
		{
			WordDictionaryTree wordTree = _loader.Load(inputStream);

			new BinaryWordTreeFile().SaveTree(outputStream, wordTree);
		}
	}
}