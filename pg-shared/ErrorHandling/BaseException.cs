using System.Diagnostics.CodeAnalysis;

namespace PG.Shared.ErrorHandling
{
	public class BaseException : Exception
	{
		public ExceptionType Type { get; set; }
		public string CorrelationId { get; set; }
		public DateTimeOffset Timestamp { get; set; }

		public BaseException() : base()
		{
			Initialize();
		}

		public BaseException(string message) : base(message)
		{
			Initialize();
		}

		public BaseException(string message, Exception innerException) : base(message, innerException)
		{
			Initialize();
		}

		[MemberNotNull(nameof(CorrelationId), nameof(Timestamp))]
		private void Initialize()
		{
			CorrelationId = Guid.NewGuid().ToString();
			Timestamp = DateTimeOffset.UtcNow;
		}
	}

	public enum ExceptionType { Application, Business }
}