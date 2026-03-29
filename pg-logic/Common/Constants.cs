using System.Text;

namespace PG.Logic.Common
{
	public static class Constants
	{
		/// <summary>
		/// Maximum allowed number of iterations to generate a valid password with the given constraints.
		/// </summary>
		public const int MAX_ITERATIONS = 10000;

		/// <summary>
		/// Coefficient used to calculate the maximum depth level based on average word length (max = √avgLength × coefficient).
		/// </summary>
		/// <remarks>
		/// Prevents "Max iterations reached" exception by ensuring depth level stays proportional as word length increases.
		/// </remarks>
		public const double DEPTH_LEVEL_COEFFICIENT = 1.34;

		public const double MIN_DEPTH_LEVEL = 2;

		internal static readonly Encoding DictionaryEncoding = Encoding.UTF8;
	}
}
