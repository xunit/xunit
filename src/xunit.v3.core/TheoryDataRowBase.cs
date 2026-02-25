namespace Xunit;

/// <summary>
/// Provide a base implemention of <see cref="ITheoryDataRow"/> with settable properties and
/// deferred data storage.
/// </summary>
public abstract partial class TheoryDataRowBase : ITheoryDataRow
{
	Dictionary<string, HashSet<string>> traits = [];

	/// <inheritdoc/>
	public bool? Explicit { get; set; }

	/// <inheritdoc/>
	public string? Label { get; set; }

	/// <inheritdoc/>
	public string? Skip { get; set; }

	/// <inheritdoc/>
	public string? TestDisplayName { get; set; }

	/// <inheritdoc/>
	public int? Timeout { get; set; }

	/// <summary>
	/// Gets or sets the traits for the theory data row.
	/// </summary>
	public Dictionary<string, HashSet<string>> Traits
	{
		get => traits;
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	/// <summary>
	/// Override to implement <see cref="ITheoryDataRow.GetData()"/>.
	/// </summary>
	protected abstract object?[] GetData();

	object?[] ITheoryDataRow.GetData() => GetData();
}
