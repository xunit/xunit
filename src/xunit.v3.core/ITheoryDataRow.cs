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
		/// Gets the theory data.
		/// </summary>
		object?[] GetData();
	}
}
