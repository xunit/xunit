#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System.Runtime.CompilerServices;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Attribute that is applied to a method to indicate that is a fact that should be run
/// by the default test runner, using one or more cultures.
/// </summary>
/// <param name="cultures">One or more cultures to run the test method under. The cultures must be valid culture names
/// that can be passed to <see cref="CultureInfo(string)"/>.</param>
/// <param name="sourceFilePath">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
/// <param name="sourceLineNumber">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
[XunitTestCaseDiscoverer(typeof(CulturedFactAttributeDiscoverer))]
public class CulturedFactAttribute(
	string[] cultures,
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		FactAttribute(sourceFilePath, sourceLineNumber)
{
	/// <summary>
	/// Gets the cultures that the test will be run under.
	/// </summary>
	public string[] Cultures => cultures;
}
