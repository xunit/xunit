#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Attribute that is applied to a method to indicate that it is a data theory that should be run
/// by the default test runner.
/// </summary>
/// <param name="sourceFilePath">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
/// <param name="sourceLineNumber">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
/// <remarks>
/// Data theories are tests which are fed data from a data source, mapping to parameters on the test method.
/// If the data source contains multiple rows, then the test method is executed multiple times (once with each
/// data row). Data is provided by attributes which implement <see cref="IDataAttribute"/> (most commonly,
/// <see cref="InlineDataAttribute"/> and <see cref="MemberDataAttribute"/>).
/// </remarks>
[XunitTestCaseDiscoverer(typeof(TheoryDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TheoryAttribute(
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		FactAttribute(sourceFilePath, sourceLineNumber), ITheoryAttribute
{
	/// <inheritdoc/>
	public bool DisableDiscoveryEnumeration { get; set; }

	/// <inheritdoc/>
	public bool SkipTestWithoutData { get; set; }
}
