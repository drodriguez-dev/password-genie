using PG.Data.Files.DataFiles.WordTrees;
using PG.Entities.Files;
using System.Text;

namespace PG.Logic.Passwords.Loader
{
	public interface IDictionaryLoaderFactory
	{
		IDictionaryLoader CreateForDictionary(DictionaryType type, Encoding encoding);
		IWordTreeData CreateForWordTree(DictionaryType type);
	}
}
