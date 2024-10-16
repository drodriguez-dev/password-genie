using PG.Data.Files.DataFiles.Dictionaries;
using PG.Data.Files.DataFiles.WordTrees;
using System.Text;

namespace PG.Data.Files.DataFiles
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
		public IDictionariesData CreateForDictionaryFile(string filePath, Encoding encoding)
		{
			string extension = Path.GetExtension(filePath);

			return extension.ToLower() switch
			{
				".txt" => new TextDictionaryFile(filePath, encoding),
				_ => throw new NotSupportedException($"File extension '{extension}' is not supported.")
			};
		}

		public IWordTreeData CreateForWordTreeFile(string filePath)
		{
			string extension = Path.GetExtension(filePath);

			return extension.ToLower() switch
			{
				".gz" => new BinaryWordTreeFile(filePath),
				_ => throw new NotSupportedException($"File extension '{extension}' is not supported.")
			};
		}
	}
}
