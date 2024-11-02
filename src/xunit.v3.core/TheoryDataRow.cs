#pragma warning disable CA2225  // We don't want to add implicit operator overloads, this is just to support TheoryData changes

using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit;

/// <summary>
/// Provide a base implemention of <see cref="ITheoryDataRow"/> with settable properties and
/// deferred data storage.
/// </summary>
public abstract class TheoryDataRowBase : ITheoryDataRow
{
	Dictionary<string, HashSet<string>> traits = [];

	/// <inheritdoc/>
	public bool? Explicit { get; set; }

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

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts untyped pre-enumerated data.
/// </summary>
/// <remarks>
/// It is strongly recommended that you use one of the generic typed variations when possible,
/// as this provides type-safety from the compiler and allows the analyzers to flag instances
/// where data types from theory data don't match the data types in theory parameters.
/// </remarks>
/// <param name="data">The data for the theory row</param>
public class TheoryDataRow(params object?[] data) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public object?[] Data =>
		data;

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		data;
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts one typed value.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1}"/> class,
/// encapsulating the one item of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
public sealed class TheoryDataRow<T1>(T1 p1) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public T1 Data =>
		p1;

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1>(T1 p1) =>
		new(p1);

	/// <summary/>
	public static implicit operator T1(TheoryDataRow<T1> p1) =>
		Guard.ArgumentNotNull(p1).Data;
}
