using PG.Data.Files.DataFiles.Dictionaries;
using PG.Data.Files.DataFiles.WordTrees;
using PG.Entities.Files;
using System.Text;

namespace PG.Data.Files.DataFiles
{
	/// <summary>
	/// Represents a factory for creating instances of <see cref="IDictionariesData"/>.
	/// </summary>
	public interface IDictionariesDataFactory
	{
		IDictionariesData CreateForDictionaryFile(DictionaryType type, Encoding encoding);
		IWordTreeData CreateForWordTreeFile(DictionaryType type);
	}
}
