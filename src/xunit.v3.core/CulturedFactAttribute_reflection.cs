#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System.Runtime.CompilerServices;

#if !XUNIT_AOT
using Xunit.v3;
#endif

namespace Xunit;

/// <summary>
/// Attribute that is applied to a method to indicate that is a fact that should be run
/// by the default test runner, using one or more cultures.
/// </summary>
/// <param name="cultures">One or more cultures to run the test method under. The cultures must be valid culture names
/// that can be passed to <see cref="CultureInfo(string)"/>.</param>
/// <param name="sourceFilePath">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
/// <param name="sourceLineNumber">This parameter is provided automatically by the compiler. Do not pass a value for it.</param>
#if !XUNIT_AOT
[XunitTestCaseDiscoverer(typeof(CulturedFactAttributeDiscoverer))]
#endif
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

#if XUNIT_AOT

	/// <summary>
	/// Invocation helper to run a test method within a specific culture.
	/// </summary>
	/// <param name="culture">The culture to switch to</param>
	/// <param name="instance">The test class instance</param>
	/// <param name="invoker">The method invoker</param>
	/// <returns></returns>
	public static async ValueTask Invoke(
		string culture,
		object? instance,
		Func<object?, ValueTask> invoker)
	{
		var originalCulture = CultureInfo.CurrentCulture;
		var originalUICulture = CultureInfo.CurrentUICulture;

		try
		{
			var newCulture = new CultureInfo(culture, useUserOverride: false);
			CultureInfo.CurrentCulture = newCulture;
			CultureInfo.CurrentUICulture = newCulture;

			await Guard.ArgumentNotNull(invoker)(instance);
		}
		finally
		{
			CultureInfo.CurrentCulture = originalCulture;
			CultureInfo.CurrentUICulture = originalUICulture;
		}
	}

#endif
}
