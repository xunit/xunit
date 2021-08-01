namespace Xunit
{
	/// <summary>
	/// Represents a single row of theory data, along with optional metadata attached to the row.
	/// </summary>
	public interface ITheoryDataRow
	{
		/// <summary>
		/// Gets the theory data.
		/// </summary>
		object?[] GetData();
	}
}
