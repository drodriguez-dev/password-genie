using System.Text;

namespace PG.Logic.Common
{
	public static class Constants
	{
		/// <summary>
		/// Maximum allowed number of iterations to generate a valid password with the given constraints.
		/// </summary>
		public const int MAX_ITERATIONS = 1000;
		public const double DEPTH_LEVEL_COEFFICIENT = 1.34;
		internal static readonly Encoding DictionaryEncoding = Encoding.UTF8;
	}
}
