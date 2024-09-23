using PG.Shared.ErrorHandling;

namespace PG.Logic.ErrorHandling
{
	public class BusinessExceptions
	{
		public class InvalidOptionException : BusinessException
		{
			public InvalidOptionException() { }
			public InvalidOptionException(string message) : base(message) { }
			public InvalidOptionException(string message, Exception innerException) : base(message, innerException) { }
		}
	}
}
