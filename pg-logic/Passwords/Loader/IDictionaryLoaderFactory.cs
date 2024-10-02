using System.Text;

namespace PG.Logic.Passwords.Loader
{
	public interface IDictionaryLoaderFactory
	{
		IDictionaryLoader Create(string filePath, Encoding encoding);
	}
}
