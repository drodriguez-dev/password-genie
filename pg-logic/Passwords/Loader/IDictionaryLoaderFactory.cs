using PG.Data.Files.DataFiles;
using PG.Data.Files.DataFiles.WordTrees;
using System.Text;

namespace PG.Logic.Passwords.Loader
{
	public interface IDictionaryLoaderFactory
	{
		IDictionaryLoader CreateForDictionary(DictionaryType type, Encoding encoding);
		IWordTreeData CreateForWordTree(DictionaryType type);
	}
}
