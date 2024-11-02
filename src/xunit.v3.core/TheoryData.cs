using System.Collections;
using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit;

/// <summary>
/// Provides a base type for theories based on collection initialization syntax.
/// </summary>
/// <typeparam name="TTheoryDataRow">The type of the theory data row (must implement <see cref="ITheoryDataRow"/>).</typeparam>
/// <typeparam name="TRawDataRow">The raw data row that requires conversion to <typeparamref name="TTheoryDataRow"/>.</typeparam>
public abstract class TheoryDataBase<TTheoryDataRow, TRawDataRow> : IReadOnlyCollection<TTheoryDataRow>
	where TTheoryDataRow : class, ITheoryDataRow
{
	readonly List<TTheoryDataRow> data = [];

	/// <inheritdoc/>
	public int Count => data.Count;

	/// <summary>
	/// Adds a row to the theory.
	/// </summary>
	/// <param name="row">The row of data to be added.</param>
	public void Add(TRawDataRow row) =>
		data.Add(Convert(row));

	/// <summary>
	/// Adds a row to the theory.
	/// </summary>
	/// <param name="row">The row of data to be added.</param>
	public void Add(TTheoryDataRow row) =>
		data.Add(Guard.ArgumentNotNull(row));

	/// <summary>
	/// Adds multiple rows to the theory.
	/// </summary>
	/// <param name="rows">The rows of data to be added.</param>
	public void AddRange(IEnumerable<TRawDataRow> rows)
	{
		foreach (var row in Guard.ArgumentNotNull(rows))
			Add(row);
	}

	/// <summary>
	/// Adds multiple rows to the theory.
	/// </summary>
	/// <param name="rows">The rows of data to be added.</param>
	public void AddRange(params TRawDataRow[] rows)
	{
		foreach (var row in Guard.ArgumentNotNull(rows))
			Add(row);
	}

	/// <summary>
	/// Adds multiple rows to the theory.
	/// </summary>
	/// <param name="rows">The rows of data to be added.</param>
	public void AddRange(IEnumerable<TTheoryDataRow> rows)
	{
		foreach (var row in Guard.ArgumentNotNull(rows))
			Add(row);
	}

	/// <summary>
	/// Adds multiple rows to the theory.
	/// </summary>
	/// <param name="rows">The rows of data to be added.</param>
	public void AddRange(params TTheoryDataRow[] rows)
	{
		foreach (var row in Guard.ArgumentNotNull(rows))
			Add(row);
	}

	/// <summary>
	/// Convert untyped values into the appropriate theory data row.
	/// </summary>
	/// <param name="row">The raw data row</param>
	protected abstract TTheoryDataRow Convert(TRawDataRow row);

	/// <inheritdoc/>
	public IEnumerator<TTheoryDataRow> GetEnumerator() => data.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Represents an untyped set of data for a theory with an unknown number
/// of parameters.
/// </summary>
/// <remarks>
/// It is strongly recommended that you use <see cref="TheoryDataBase{TTheoryDataRow, TRawDataRow}"/>
/// as the base type for any strongly typed collections, because it will provide both
/// strong compiler support and wider collection initialization syntax support.
/// </remarks>
public abstract class TheoryData : TheoryDataBase<TheoryDataRow, object?[]>
{
	/// <inheritdoc/>
	protected override TheoryDataRow Convert(object?[] row) =>
		new(row);
}

/// <summary>
/// Represents a set of data for a theory with a single parameter. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T">The parameter type.</typeparam>
public class TheoryData<T> : TheoryDataBase<TheoryDataRow<T>, T>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<T> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params T[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T>[] values) =>
		AddRange(values);

	/// <inheritdoc/>
	protected override TheoryDataRow<T> Convert(T row) =>
		new(row);
}
