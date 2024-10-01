namespace PG.Shared.ErrorHandling
{
	public abstract class BusinessException : BaseException
	{
		protected BusinessException() : base()
		{
			Type = ExceptionType.Business;
		}

		protected BusinessException(string message) : base(message)
		{
			Type = ExceptionType.Business;
		}

		protected BusinessException(string message, Exception innerException) : base(message, innerException)
		{
			Type = ExceptionType.Business;
		}
	}
}
