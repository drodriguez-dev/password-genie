using PG.Data.Files.DataFiles.Dictionaries;

namespace PG.Tests.Business.Passwords.Generators.Mockups
{
	internal class DictionaryDataMockup(string[] words) : IDictionariesData
	{
		private readonly string[] _words = words;

		public IEnumerable<string> FetchAllWords(Stream file) => _words;
	}
}
