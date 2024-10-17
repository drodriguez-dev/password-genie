using System.Text;
using static PG.Data.Files.ErrorHandling.DataExceptions;

namespace PG.Data.Files.DataFiles.Dictionaries
{
	public class TextDictionaryFile(Encoding encoding) : IDictionariesData
	{
		private readonly Encoding _encoding = encoding;

		public IEnumerable<string> FetchAllWords(Stream fileStream)
		{
			if (fileStream == null)
				throw new InvalidPathFileException("Dictionary file path was not provided.");

			if (fileStream.Length == 0)
				throw new InvalidFileException("Dictionary file is empty.");

			return YieldAllLines(fileStream);
		}

		private IEnumerable<string> YieldAllLines(Stream fileStream)
		{
			// There is no block of code of the using statement because the yield return statement suspends the execution of the method, but it does not
			// exit the method, and StreamReader object needs to remain valid throughout the execution of the method. The StreamReader object will be
			// disposed of automatically when the method finishes executing.
			using StreamReader reader = new(fileStream, _encoding);

			string? line;
			while ((line = reader.ReadLine()) != null)
				yield return line.ToLower();
		}
	}
}
