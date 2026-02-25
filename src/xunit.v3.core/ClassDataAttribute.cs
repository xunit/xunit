#pragma warning disable CA1813  // This attribute is unsealed because it's an extensibility point

using Xunit.v3;

namespace Xunit;

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
/// <param name="class">The class that provides the data.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public partial class ClassDataAttribute(Type @class) : DataAttribute
{
	/// <summary>
	/// Gets the type of the class that provides the data.
	/// </summary>
	public Type Class { get; } = @class;
}
