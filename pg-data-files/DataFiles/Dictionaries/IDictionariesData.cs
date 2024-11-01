namespace PG.Data.Files.DataFiles.Dictionaries
{
	/// <summary>
	/// Represents a data source for dictionaries to get words from.
	/// </summary>
	public interface IDictionariesData
	{
		IEnumerable<string> FetchAllWords(Stream fileStream);
	}
}
