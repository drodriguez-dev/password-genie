namespace PG.Logic.Passwords.Extractors
{
	public interface IWordExtractor
	{
		void ExtractWordTree(Stream inputStream, Stream outputStream);
	}
}
