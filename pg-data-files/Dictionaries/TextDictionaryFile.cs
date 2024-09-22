using System.Text;

namespace PG.Data.Files.Dictionaries
{
	public class TextDictionaryFile(string filePath, Encoding encoding) : IDictionariesData
	{
		public string FilePath { get; set; } = filePath;

		public Encoding Encoding { get; set; } = encoding;

		public IEnumerable<string> GetWords()
		{
			if (string.IsNullOrEmpty(FilePath))
				throw new ArgumentNullException("File path not provided.");

			using StreamReader reader = new(FilePath, Encoding);

			string? line;
			while ((line = reader.ReadLine()) != null)
				yield return line.ToLower();
		}
	}
}
