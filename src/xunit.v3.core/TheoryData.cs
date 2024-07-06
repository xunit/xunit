using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

/// <summary>
/// Represents a set of data for a theory with 2 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
public class TheoryData<T1, T2> : TheoryDataBase<TheoryDataRow<T1, T2>, (T1, T2)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	public void Add(T1 p1, T2 p2) =>
		Add(new TheoryDataRow<T1, T2>(p1, p2));

	/// <inheritdoc/>
	protected override TheoryDataRow<T1, T2> Convert((T1, T2) row) =>
		new(row.Item1, row.Item2);
}

/// <summary>
/// Represents a set of data for a theory with 3 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
public class TheoryData<T1, T2, T3> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3>, (T1, T2, T3)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	/// <param name="p3">The third data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3) =>
		Add(new TheoryDataRow<T1, T2, T3>(p1, p2, p3));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3> Convert((T1, T2, T3) row) =>
		new(row.Item1, row.Item2, row.Item3);
}

/// <summary>
/// Represents a set of data for a theory with 4 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4>, (T1, T2, T3, T4)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	/// <param name="p3">The third data value.</param>
	/// <param name="p4">The fourth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4) =>
		Add(new TheoryDataRow<T1, T2, T3, T4>(p1, p2, p3, p4));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4> Convert((T1, T2, T3, T4) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4);
}

/// <summary>
/// Represents a set of data for a theory with 5 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5>, (T1, T2, T3, T4, T5)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	/// <param name="p3">The third data value.</param>
	/// <param name="p4">The fourth data value.</param>
	/// <param name="p5">The fifth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5>(p1, p2, p3, p4, p5));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5> Convert((T1, T2, T3, T4, T5) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5);
}

/// <summary>
/// Represents a set of data for a theory with 6 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6>, (T1, T2, T3, T4, T5, T6)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	/// <param name="p3">The third data value.</param>
	/// <param name="p4">The fourth data value.</param>
	/// <param name="p5">The fifth data value.</param>
	/// <param name="p6">The sixth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6>(p1, p2, p3, p4, p5, p6));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6> Convert((T1, T2, T3, T4, T5, T6) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6);
}

/// <summary>
/// Represents a set of data for a theory with 7 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7>, (T1, T2, T3, T4, T5, T6, T7)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	/// <param name="p3">The third data value.</param>
	/// <param name="p4">The fourth data value.</param>
	/// <param name="p5">The fifth data value.</param>
	/// <param name="p6">The sixth data value.</param>
	/// <param name="p7">The seventh data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7>(p1, p2, p3, p4, p5, p6, p7));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7> Convert((T1, T2, T3, T4, T5, T6, T7) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7);
}

/// <summary>
/// Represents a set of data for a theory with 8 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
/// <typeparam name="T8">The eighth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8>, (T1, T2, T3, T4, T5, T6, T7, T8)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7, T8)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	/// <param name="p3">The third data value.</param>
	/// <param name="p4">The fourth data value.</param>
	/// <param name="p5">The fifth data value.</param>
	/// <param name="p6">The sixth data value.</param>
	/// <param name="p7">The seventh data value.</param>
	/// <param name="p8">The eighth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8>(p1, p2, p3, p4, p5, p6, p7, p8));

	/// <inheritdoc/>
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8> Convert((T1, T2, T3, T4, T5, T6, T7, T8) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8);
}

/// <summary>
/// Represents a set of data for a theory with 9 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
/// <typeparam name="T8">The eighth parameter type.</typeparam>
/// <typeparam name="T9">The ninth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>, (T1, T2, T3, T4, T5, T6, T7, T8, T9)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7, T8, T9)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	/// <param name="p3">The third data value.</param>
	/// <param name="p4">The fourth data value.</param>
	/// <param name="p5">The fifth data value.</param>
	/// <param name="p6">The sixth data value.</param>
	/// <param name="p7">The seventh data value.</param>
	/// <param name="p8">The eighth data value.</param>
	/// <param name="p9">The ninth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(p1, p2, p3, p4, p5, p6, p7, p8, p9));

	/// <inheritdoc/>
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9> Convert((T1, T2, T3, T4, T5, T6, T7, T8, T9) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9);
}

/// <summary>
/// Represents a set of data for a theory with 10 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
/// <typeparam name="T8">The eighth parameter type.</typeparam>
/// <typeparam name="T9">The ninth parameter type.</typeparam>
/// <typeparam name="T10">The tenth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> values)
	{
		Guard.ArgumentNotNull(values);

		AddRange(values.ToArray());
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>[] values) =>
		AddRange(values);

	/// <summary>
	/// Adds data to the theory data set.
	/// </summary>
	/// <param name="p1">The first data value.</param>
	/// <param name="p2">The second data value.</param>
	/// <param name="p3">The third data value.</param>
	/// <param name="p4">The fourth data value.</param>
	/// <param name="p5">The fifth data value.</param>
	/// <param name="p6">The sixth data value.</param>
	/// <param name="p7">The seventh data value.</param>
	/// <param name="p8">The eighth data value.</param>
	/// <param name="p9">The ninth data value.</param>
	/// <param name="p10">The tenth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10));

	/// <inheritdoc/>
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Convert((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10);
}
