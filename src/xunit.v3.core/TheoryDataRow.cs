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

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts two typed values.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2}"/> class,
/// encapsulating the two items of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
/// <param name="p2">The second data value.</param>
public sealed class TheoryDataRow<T1, T2>(T1 p1, T2 p2) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2) Data =>
		(p1, p2);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2>((T1, T2) row) =>
		new(row.Item1, row.Item2);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts three typed values.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3}"/> class,
/// encapsulating the three items of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
/// <param name="p2">The second data value.</param>
/// <param name="p3">The third data value.</param>
public sealed class TheoryDataRow<T1, T2, T3>(T1 p1, T2 p2, T3 p3) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3) Data =>
		(p1, p2, p3);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3>((T1, T2, T3) row) =>
		new(row.Item1, row.Item2, row.Item3);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts four typed values.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4}"/> class,
/// encapsulating the four items of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
/// <param name="p2">The second data value.</param>
/// <param name="p3">The third data value.</param>
/// <param name="p4">The fourth data value.</param>
public sealed class TheoryDataRow<T1, T2, T3, T4>(T1 p1, T2 p2, T3 p3, T4 p4) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4) Data =>
		(p1, p2, p3, p4);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4>((T1, T2, T3, T4) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts five typed values.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5}"/> class,
/// encapsulating the five items of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
/// <param name="p2">The second data value.</param>
/// <param name="p3">The third data value.</param>
/// <param name="p4">The fourth data value.</param>
/// <param name="p5">The fifth data value.</param>
public sealed class TheoryDataRow<T1, T2, T3, T4, T5>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5) Data =>
		(p1, p2, p3, p4, p5);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5>((T1, T2, T3, T4, T5) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts six typed values.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6}"/> class,
/// encapsulating the six items of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
/// <param name="p2">The second data value.</param>
/// <param name="p3">The third data value.</param>
/// <param name="p4">The fourth data value.</param>
/// <param name="p5">The fifth data value.</param>
/// <param name="p6">The sixth data value.</param>
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6) Data =>
		(p1, p2, p3, p4, p5, p6);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6>((T1, T2, T3, T4, T5, T6) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts seven typed values.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7}"/> class,
/// encapsulating the seven items of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
/// <param name="p2">The second data value.</param>
/// <param name="p3">The third data value.</param>
/// <param name="p4">The fourth data value.</param>
/// <param name="p5">The fifth data value.</param>
/// <param name="p6">The sixth data value.</param>
/// <param name="p7">The seventh data value.</param>
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7) Data =>
		(p1, p2, p3, p4, p5, p6, p7);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7>((T1, T2, T3, T4, T5, T6, T7) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts eight typed values.
/// </summary>
/// <typeparam name="T1">The first parameter type.</typeparam>
/// <typeparam name="T2">The second parameter type.</typeparam>
/// <typeparam name="T3">The third parameter type.</typeparam>
/// <typeparam name="T4">The fourth parameter type.</typeparam>
/// <typeparam name="T5">The fifth parameter type.</typeparam>
/// <typeparam name="T6">The sixth parameter type.</typeparam>
/// <typeparam name="T7">The seventh parameter type.</typeparam>
/// <typeparam name="T8">The eighth parameter type.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7, T8}"/> class,
/// encapsulating the eight items of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
/// <param name="p2">The second data value.</param>
/// <param name="p3">The third data value.</param>
/// <param name="p4">The fourth data value.</param>
/// <param name="p5">The fifth data value.</param>
/// <param name="p6">The sixth data value.</param>
/// <param name="p7">The seventh data value.</param>
/// <param name="p8">The eighth data value.</param>
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7, T8) Data =>
		(p1, p2, p3, p4, p5, p6, p7, p8);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7, p8];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8>((T1, T2, T3, T4, T5, T6, T7, T8) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts nine typed values.
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
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7, T8, T9}"/> class,
/// encapsulating the nine items of typed data.
/// </remarks>
/// <param name="p1">The first data value.</param>
/// <param name="p2">The second data value.</param>
/// <param name="p3">The third data value.</param>
/// <param name="p4">The fourth data value.</param>
/// <param name="p5">The fifth data value.</param>
/// <param name="p6">The sixth data value.</param>
/// <param name="p7">The seventh data value.</param>
/// <param name="p8">The eighth data value.</param>
/// <param name="p9">The ninth data value.</param>
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9) Data =>
		(p1, p2, p3, p4, p5, p6, p7, p8, p9);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7, p8, p9];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9>((T1, T2, T3, T4, T5, T6, T7, T8, T9) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts ten typed values.
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
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10}"/> class,
/// encapsulating the ten items of typed data.
/// </remarks>
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
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) Data =>
		(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7, p8, p9, p10];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts eleven typed values.
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
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11}"/> class,
/// encapsulating the eleven items of typed data.
/// </remarks>
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
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11) Data =>
		(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts twelve typed values.
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
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12}"/> class,
/// encapsulating the twelve items of typed data.
/// </remarks>
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
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12) Data =>
		(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11, row.Item12);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts thirteen typed values.
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
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13}"/> class,
/// encapsulating the thirteen items of typed data.
/// </remarks>
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
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13) Data =>
		(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11, row.Item12, row.Item13);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts fourteen typed values.
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
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14}"/> class,
/// encapsulating the fourteen items of typed data.
/// </remarks>
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
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14) Data =>
		(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11, row.Item12, row.Item13, row.Item14);
}

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts fifteen typed values.
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
/// <remarks>
/// Initializes a new instance of the <see cref="TheoryDataRow{T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15}"/> class,
/// encapsulating the fifteen items of typed data.
/// </remarks>
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
public sealed class TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15) :
	TheoryDataRowBase
{
	/// <summary>
	/// Gets the row of data.
	/// </summary>
	public (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15) Data =>
		(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);

	/// <inheritdoc/>
	protected override object?[] GetData() =>
		[p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15];

	/// <summary/>
	public static implicit operator TheoryDataRow<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15) row) =>
		new(row.Item1, row.Item2, row.Item3, row.Item4, row.Item5, row.Item6, row.Item7, row.Item8, row.Item9, row.Item10, row.Item11, row.Item12, row.Item13, row.Item14, row.Item15);
}
