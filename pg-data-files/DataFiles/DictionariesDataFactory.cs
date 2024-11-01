using PG.Data.Files.DataFiles.Dictionaries;
using PG.Data.Files.DataFiles.WordTrees;
using PG.Entities.Files;
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
		public IDictionariesData CreateForDictionaryFile(DictionaryType type, Encoding encoding)
		{
			return type switch
			{
				DictionaryType.PlainTextDictionary => new TextDictionaryFile(encoding),
				_ => throw new NotSupportedException($"Dictionary type '{type}' is not supported for dictionary files."),
			};
		}

		public IWordTreeData CreateForWordTreeFile(DictionaryType type)
		{
			return type switch
			{
				DictionaryType.WordTree => new BinaryWordTreeFile(),
				_ => throw new NotSupportedException($"Dictionary type '{type}' is not supported for dictionary files."),
			};
		}
	}
}
