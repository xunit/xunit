using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Default implementation of <see cref="ITheoryDataRow"/> which accepts pre-enumerated data.
/// </summary>
public class TheoryDataRow : ITheoryDataRow
{
	readonly object?[] data;
	Dictionary<string, List<string>> traits = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryDataRow"/> class, encapsulating the
	/// provided theory data.
	/// </summary>
	/// <param name="data">The data for the theory row</param>
	public TheoryDataRow(params object?[] data)
	{
		this.data = Guard.ArgumentNotNull(data);
	}

	/// <inheritdoc/>
	public bool? Explicit { get; set; }

	/// <inheritdoc/>
	public string? Skip { get; set; }

	/// <inheritdoc/>
	public string? TestDisplayName { get; set; }

	/// <inheritdoc/>
	public int? Timeout { get; set; }

#pragma warning disable CA2227 // This is purposefully writable because of (de)serialization

	/// <summary>
	/// Gets or sets the traits for the theory data row.
	/// </summary>
	public Dictionary<string, List<string>> Traits
	{
		get => traits;
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

#pragma warning restore CA2227

	/// <summary>
	/// Adds a single trait name/value pair to the theory data row.
	/// </summary>
	/// <param name="name">The name of the trait</param>
	/// <param name="value">The value of the trait</param>
	public TheoryDataRow WithTrait(
		string name,
		string value)
	{
		var list = Traits.GetOrAdd(name, () => new());
		list.Add(value);
		return this;
	}

	/// <inheritdoc/>
	public object?[] GetData() => data;
}
