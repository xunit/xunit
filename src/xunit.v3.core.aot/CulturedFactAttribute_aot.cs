#pragma warning disable CA1019  // Constructor value accessors are not needed as this is only consumed during code generation

using System.Runtime.CompilerServices;

namespace Xunit;

/// <summary>
/// Attribute that is applied to a method to indicate that is a fact that should be run
/// by the default test runner, using one or more cultures.
/// </summary>
/// <param name="cultures">One or more cultures to run the test method under. The cultures must be valid culture names
/// that can be passed to <see cref="CultureInfo(string)"/>.</param>
/// <param name="sourceFilePath">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
/// <param name="sourceLineNumber">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
public sealed class CulturedFactAttribute(
	string[] cultures,
	[CallerFilePath] string? sourceFilePath = null,
	[CallerLineNumber] int sourceLineNumber = -1) :
		FactAttributeBase
{ }
