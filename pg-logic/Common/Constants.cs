using System.Text;

namespace PG.Logic.Common
{
	internal static class Constants
	{
		/// <summary>
		/// Maximum allowed number of iterations to generate a valid password with the given constraints.
		/// </summary>
		public const int MAX_ITERATIONS = 1000;
		internal static readonly Encoding DictionaryEncoding = Encoding.UTF8;
	}
}
