using PG.Data.Files.DataFiles.WordTrees;
using System.Text;

namespace PG.Logic.Passwords.Loader
{
	public interface IDictionaryLoaderFactory
	{
		IDictionaryLoader CreateForDictionary(string filePath, Encoding encoding);
		IWordTreeData CreateForWordTree(string filePath);
	}
}
