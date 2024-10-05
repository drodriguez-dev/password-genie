using System.Text.RegularExpressions;

namespace PG.Tests.Business.Passwords.Generators
{
	public abstract partial class PasswordGeneratorTestBase
	{
		[GeneratedRegex(@"\w+")]
		protected static partial Regex WordPattern();
		[GeneratedRegex(@"[a-zA-Z]")]
		protected static partial Regex LettersPattern();
		[GeneratedRegex(@"[yuiophjklnmYUIOPHJKLNM]")]
		protected static partial Regex RightHandPattern();
		[GeneratedRegex(@"[qwertasdfgzxcvbQWERTASDFGZXCVB]")]
		protected static partial Regex LeftHandPattern();
	}
}