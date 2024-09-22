namespace PG.Data.Files.Dictionaries
{
	/// <summary>
	/// Represents a data source for dictionaries to get words from.
	/// </summary>
	public interface IDictionariesData
	{
		IEnumerable<string> GetWords();
	}
}
