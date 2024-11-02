using System.Collections.Generic;

namespace Xunit;

/// <summary>
/// Represents a set of data for a theory with 2 parameters. Data can
/// be added to the data set using the collection initializer syntax.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
public class TheoryData<T1, T2> :
	TheoryDataBase<TheoryDataRow<T1, T2>, (T1, T2)>
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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

	/// <inheritdoc />
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
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> values) =>
		AddRange(values);

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
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> values) =>
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

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Convert((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10);
}

/// <summary>
/// Represents a set of data for a theory with 11 parameters. Data can
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
/// <typeparam name="T11">The eleventh parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>[] values) =>
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
	/// <param name="p11">The eleventh data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Convert((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11);
}

/// <summary>
/// Represents a set of data for a theory with 12 parameters. Data can
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
/// <typeparam name="T11">The eleventh parameter type.</typeparam>
/// <typeparam name="T12">The twelfth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>[] values) =>
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
	/// <param name="p11">The eleventh data value.</param>
	/// <param name="p12">The twelfth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Convert((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11, row.Item12);
}

/// <summary>
/// Represents a set of data for a theory with 13 parameters. Data can
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
/// <typeparam name="T11">The eleventh parameter type.</typeparam>
/// <typeparam name="T12">The twelfth parameter type.</typeparam>
/// <typeparam name="T13">The thirteenth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>[] values) =>
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
	/// <param name="p11">The eleventh data value.</param>
	/// <param name="p12">The twelfth data value.</param>
	/// <param name="p13">The thirteenth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Convert((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11, row.Item12, row.Item13);
}

/// <summary>
/// Represents a set of data for a theory with 14 parameters. Data can
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
/// <typeparam name="T11">The eleventh parameter type.</typeparam>
/// <typeparam name="T12">The twelfth parameter type.</typeparam>
/// <typeparam name="T13">The thirteenth parameter type.</typeparam>
/// <typeparam name="T14">The fourteenth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>[] values) =>
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
	/// <param name="p11">The eleventh data value.</param>
	/// <param name="p12">The twelfth data value.</param>
	/// <param name="p13">The thirteenth data value.</param>
	/// <param name="p14">The fourteenth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Convert((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11, row.Item12, row.Item13, row.Item14);
}

/// <summary>
/// Represents a set of data for a theory with 15 parameters. Data can
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
/// <typeparam name="T11">The eleventh parameter type.</typeparam>
/// <typeparam name="T12">The twelfth parameter type.</typeparam>
/// <typeparam name="T13">The thirteenth parameter type.</typeparam>
/// <typeparam name="T14">The fourteenth parameter type.</typeparam>
/// <typeparam name="T15">The fifteenth parameter type.</typeparam>
public class TheoryData<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> :
	TheoryDataBase<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/> class.
	/// </summary>
	public TheoryData()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)[] values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(IEnumerable<TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>> values) =>
		AddRange(values);

	/// <summary>
	/// Initializes a new instance of the <see cref="TheoryData{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/> class.
	/// </summary>
	/// <param name="values">The initial set of values</param>
	public TheoryData(params TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>[] values) =>
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
	/// <param name="p11">The eleventh data value.</param>
	/// <param name="p12">The twelfth data value.</param>
	/// <param name="p13">The thirteenth data value.</param>
	/// <param name="p14">The fourteenth data value.</param>
	/// <param name="p15">The fifteenth data value.</param>
	public void Add(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15) =>
		Add(new TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15));

	/// <inheritdoc />
	protected override TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Convert((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11, row.Item12, row.Item13, row.Item14, row.Item15);
}
