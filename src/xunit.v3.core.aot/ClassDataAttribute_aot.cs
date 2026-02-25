using Xunit.v3;

namespace Xunit;

sealed partial class ClassDataAttribute
{ }

/// <summary>
/// Provides a data source for a data theory, with the data coming from a class
/// which must implement <see cref="IEnumerable{T}"/> or <see cref="IAsyncEnumerable{T}"/>
/// of one of:
/// <list type="bullet">
/// <item><c><see cref="object"/>?[]</c></item>
/// <item><c><see cref="ITheoryDataRow"/></c></item>
/// <item><c><see cref="T:System.Runtime.CompilerServices.ITuple"/></c></item>
/// </list>
/// </summary>
/// <typeparam name="TClass">The class that provides the data.</typeparam>
/// <remarks>
/// .NET Framework does not support generic attributes. Please use the non-generic <see cref="ClassDataAttribute"/>
/// when targeting .NET Framework.
/// </remarks>
public sealed class ClassDataAttribute<TClass>() : DataAttribute
{ }
