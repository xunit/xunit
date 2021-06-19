using System.Text.RegularExpressions;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// Utilities for reading configuration values.
	/// </summary>
	public static class ConfigUtility
	{
		/// <summary>
		/// Gets the regular expression that matches the multiplier-style value for maximum
		/// parallel threads (that is, '0.5x', '2x', etc.).
		/// </summary>
		public static readonly Regex MultiplierStyleMaxParallelThreadsRegex = new("^(\\d+(\\.\\d+)?)(x|X)$");
	}
}
