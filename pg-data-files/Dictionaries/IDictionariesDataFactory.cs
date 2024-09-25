using System.Text;

namespace PG.Data.Files.Dictionaries
{
	/// <summary>
	/// Represents a factory for creating instances of <see cref="IDictionariesData"/>.
	/// </summary>
	public interface IDictionariesDataFactory
	{
		IDictionariesData CreateForFile(string filePath, Encoding encoding);
	}
}
