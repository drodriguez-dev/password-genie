using System.Text;
using static PG.Data.Files.ErrorHandling.DataExceptions;

namespace PG.Data.Files.Dictionaries
{
	internal class TextDictionaryFile(string filePath, Encoding encoding) : IDictionariesData
	{
		private string FilePath { get; set; } = filePath;

		private Encoding Encoding { get; set; } = encoding;

		public IEnumerable<string> FetchAllWords()
		{
			if (string.IsNullOrEmpty(FilePath))
				throw new ArgumentNullException("File path not provided.");

			try
			{
				return YieldAllLines();
			}
			catch (FileNotFoundException)
			{
				throw new InvalidFileException($"File path \"{FilePath}\" was not found.");
			}
		}

		private IEnumerable<string> YieldAllLines()
		{
			using StreamReader reader = new(FilePath, Encoding);

			string? line;
			while ((line = reader.ReadLine()) != null)
				yield return line.ToLower();
		}
	}
}
