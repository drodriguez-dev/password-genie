namespace PG.Shared.ErrorHandling
{
	public abstract class BusinessException : BaseException
	{
		public BusinessException() : base()
		{
			Type = ExceptionType.Business;
		}

		public BusinessException(string message) : base(message)
		{
			Type = ExceptionType.Business;
		}

		public BusinessException(string message, Exception innerException) : base(message, innerException)
		{
			Type = ExceptionType.Business;
		}
	}
}
