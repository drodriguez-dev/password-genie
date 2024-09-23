using System.Text;

namespace PG.Data.Files.Dictionaries
{
	/// <summary>
	/// Factory for creating instances of <see cref="IDictionariesData"/>.
	/// </summary>
	public class DictionariesDataFactory : IDictionariesDataFactory
	{
		/// <summary>
		/// Creates an instance of <see cref="IDictionariesData"/> for the specified file depending on its extension.
		/// </summary>
		/// <exception cref="NotSupportedException">Thrown when the file extension is not supported.</exception>
		public IDictionariesData CreateForFile(string filePath, Encoding encoding)
		{
			string extension = Path.GetExtension(filePath);

			return extension.ToLower() switch
			{
				".txt" => new TextDictionaryFile(filePath, encoding),
				//".csv" => new CsvDictionariesData(filePath, encoding),
				_ => throw new NotSupportedException($"File extension '{extension}' is not supported.")
			};
		}
	}
}
