using System.Collections.Generic;

namespace Xunit
{
	/// <summary>
	/// Represents a single row of theory data, along with optional metadata attached to the row.
	/// </summary>
	public interface ITheoryDataRow
	{
		/// <summary>
		/// Gets the reason for skipping this row of data; if <c>null</c> is returned, then the data
		/// row isn't skipped.
		/// </summary>
		string? Skip { get; }

		/// <summary>
		/// Gets the trait values associated with this theory data row. If there are none, you may either
		/// return a <c>null</c> or empty dictionary.
		/// </summary>
		Dictionary<string, List<string>>? Traits { get; }

		/// <summary>
		/// Gets the theory data.
		/// </summary>
		object?[] GetData();
	}
}
