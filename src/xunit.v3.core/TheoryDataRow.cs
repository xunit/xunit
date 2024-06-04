using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit;

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts untyped pre-enumerated data.
/// </summary>
/// <remarks>
/// It is strongly recommended that you use one of the generic typed variations when possible,
/// as this provides type-safety from the compiler and allows the analyzers to flag instances
/// where data types from theory data don't match the data types in theory parameters.
/// </remarks>
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

	/// <summary>
	/// Gets or sets the traits for the theory data row.
	/// </summary>
	public Dictionary<string, List<string>> Traits
	{
		get => traits;
		set => traits = Guard.ArgumentNotNull(value, nameof(Traits));
	}

	/// <inheritdoc/>
	public object?[] GetData() => data;
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
	TheoryDataRow(p1)
{ }

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
	TheoryDataRow(p1, p2)
{ }

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
	TheoryDataRow(p1, p2, p3)
{ }

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
	TheoryDataRow(p1, p2, p3, p4)
{ }

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
	TheoryDataRow(p1, p2, p3, p4, p5)
{ }

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
	TheoryDataRow(p1, p2, p3, p4, p5, p6)
{ }

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
	TheoryDataRow(p1, p2, p3, p4, p5, p6, p7)
{ }

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
	TheoryDataRow(p1, p2, p3, p4, p5, p6, p7, p8)
{ }

/// <summary>
/// Implementation of <see cref="ITheoryDataRow"/> which accepts nne typed values.
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
	TheoryDataRow(p1, p2, p3, p4, p5, p6, p7, p8, p9)
{ }

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
	TheoryDataRow(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10)
{ }
