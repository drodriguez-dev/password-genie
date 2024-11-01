using PG.Entities.WordTrees;

namespace PG.Logic.Passwords.Loaders
{
	public interface IDictionaryLoader
	{
		WordDictionaryTree Load(Stream file);
	}
}
