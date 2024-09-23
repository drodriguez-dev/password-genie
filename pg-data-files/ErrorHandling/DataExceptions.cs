using PG.Shared.ErrorHandling;

namespace PG.Data.Files.ErrorHandling
{
	public class DataExceptions
	{
		public class InvalidFileException : BusinessException
		{
			public InvalidFileException() : base() { }
			public InvalidFileException(string message) : base(message) { }
			public InvalidFileException(string message, Exception innerException) : base(message, innerException) { }
		}
	}
}
