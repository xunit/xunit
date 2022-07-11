using System.Collections.Generic;

namespace Xunit;

/// <summary>
/// Represents a single row of theory data, along with optional metadata attached to the row.
/// </summary>
public interface ITheoryDataRow
{
	/// <summary>
	/// Gets a flag that indicates that the data row should only be run explicitly. If the value is <c>null</c>,
	/// then it inherits its explicitness from the value of <see cref="FactAttribute.Explicit"/>.
	/// </summary>
	bool? Explicit { get; }

	/// <summary>
	/// Gets the reason for skipping this row of data; if <c>null</c> is returned, then the data
	/// row isn't skipped.
	/// </summary>
	string? Skip { get; }

	/// <summary>
	/// Gets the display name for the test (replacing the default behavior, which would be to use the DisplayName
	/// from <see cref="FactAttribute"/>, or falling back to the class &amp; method name).
	/// </summary>
	string? TestDisplayName { get; }

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
