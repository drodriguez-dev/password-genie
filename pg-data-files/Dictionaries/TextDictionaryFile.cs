using System.Text;
using static PG.Data.Files.ErrorHandling.DataExceptions;

namespace PG.Data.Files.Dictionaries
{
	public class TextDictionaryFile(string filePath, Encoding encoding) : IDictionariesData
	{
		private string FilePath { get; set; } = filePath;

		private Encoding Encoding { get; set; } = encoding;

		public IEnumerable<string> FetchAllWords()
		{
			if (string.IsNullOrEmpty(FilePath))
				throw new InvalidPathFileException("Dictionary file path was not provided.");

			if (!File.Exists(FilePath))
				throw new FileNotFoundException($"Dictionary file was not found ('{FilePath}').");

			return YieldAllLines();
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
