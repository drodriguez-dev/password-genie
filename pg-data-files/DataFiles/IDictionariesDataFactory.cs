using PG.Data.Files.DataFiles.Dictionaries;
using PG.Data.Files.DataFiles.WordTrees;
using System.Text;

namespace PG.Data.Files.DataFiles
{
	/// <summary>
	/// Represents a factory for creating instances of <see cref="IDictionariesData"/>.
	/// </summary>
	public interface IDictionariesDataFactory
	{
		IDictionariesData CreateForDictionaryFile(string filePath, Encoding encoding);
		IWordTreeData CreateForWordTreeFile(string filePath);
	}
}
