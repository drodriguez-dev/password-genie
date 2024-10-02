namespace PG.Shared.Extensions
{
	public static class TextExtensions
	{
		private static readonly HashSet<char> WhitespaceCharacters = [
			' ', '\t', '\n', '\r', '\v', '\f',
			'\u00A0', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u200B', '\u202F', '\u205F', '\u3000'
		];

		/// <summary>
		/// Determines if a character is printable, includes the high ASCII characters.
		/// </summary>
		public static bool IsPrintable(this char c) => (c > 32 && c <= 126) || c >= 128 && c <= 255;

		/// <summary>
		/// Determines if a character is any type of space
		/// </summary>
		public static bool IsWhitespace(this char c) => WhitespaceCharacters.Contains(c);

		/// <summary>
		/// Returns the rightmost n characters of a string.
		/// </summary>
		public static string Right(this string value, int length)
		{
			if (length <= 0)
				throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than zero.");

			if (string.IsNullOrEmpty(value))
				return value;

			if (length >= value.Length)
				return value;

			return value[^length..];
		}
	}
}
