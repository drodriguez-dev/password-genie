using PG.Shared.ErrorHandling;

namespace PG.Data.Files.ErrorHandling
{
	public class DataExceptions
	{
		public class InvalidPathFileException : BusinessException
		{
			public InvalidPathFileException() : base() { }
			public InvalidPathFileException(string message) : base(message) { }
			public InvalidPathFileException(string message, Exception innerException) : base(message, innerException) { }
		}
		public class InvalidFileException : BusinessException
		{
			public InvalidFileException() : base() { }
			public InvalidFileException(string message) : base(message) { }
			public InvalidFileException(string message, Exception innerException) : base(message, innerException) { }
		}
	}
}
