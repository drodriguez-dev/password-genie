using PG.Entities.WordTrees;

namespace PG.Logic.Passwords.Loader
{
	public interface IDictionaryLoader
	{
		WordDictionaryTree Load();
	}
}
