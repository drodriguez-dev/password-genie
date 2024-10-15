using System.Globalization;

namespace PG.Shared.Extensions
{
	public static class TextExtensions
	{
		private static readonly HashSet<char> WhitespaceCharacters = new([
		  ' ', '\t', '\n', '\r', '\v', '\f',
		  '\u00A0', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u200B', '\u202F', '\u205F', '\u3000'
		]);

		/// <summary>
		/// Determines if a character is printable, includes the high ASCII characters.
		/// </summary>
		/// <param name="c">The character to check.</param>
		/// <returns><c>true</c> if the character is printable; otherwise, <c>false</c>.</returns>
		public static bool IsPrintable(this char c) => (c > 32 && c <= 126) || c >= 128 && c <= 255;

		/// <summary>
		/// Determines if a character is any type of space.
		/// </summary>
		/// <param name="c">The character to check.</param>
		/// <returns><c>true</c> if the character is a whitespace; otherwise, <c>false</c>.</returns>
		public static bool IsWhitespace(this char c) => WhitespaceCharacters.Contains(c);

		/// <summary>
		/// Returns the rightmost n characters of a string.
		/// </summary>
		/// <param name="value">The input string.</param>
		/// <param name="length">The number of characters to return.</param>
		/// <returns>The rightmost n characters of the input string.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the length is less than or equal to zero.</exception>
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

		/// <summary>
		/// Gets the text elements of a string.
		/// </summary>
		/// <param name="text">The input string.</param>
		/// <returns>An enumerable collection of text elements.</returns>
		public static IEnumerable<string> GetTextElements(this string text)
		{
			TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
			while (enumerator.MoveNext())
				yield return enumerator.GetTextElement();
		}
	}
}
