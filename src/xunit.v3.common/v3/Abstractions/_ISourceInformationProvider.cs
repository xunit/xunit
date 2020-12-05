using System;

namespace Xunit.v3
{
	/// <summary>
	/// Represents a provider which gives source line information for a test case. Generally
	/// consumed by an implementation of <see cref="_ITestFrameworkDiscoverer"/> during Find operations.
	/// Implementations may optionally implement <see cref="IDisposable"/> and/or <see cref="IAsyncDisposable"/>
	/// for cleanup operations.
	/// </summary>
	public interface _ISourceInformationProvider
	{
		/// <summary>
		/// Returns the source information for a test case.
		/// </summary>
		/// <param name="testClassName">The test class name, if known</param>
		/// <param name="testMethodName">The test method name, if known</param>
		/// <returns>The source information, with null string and int values when the information is not available.
		/// Note: return value should never be <c>null</c>, only the interior data values inside.</returns>
		// TODO: Can this return a named tuple instead of an interface?
		_ISourceInformation GetSourceInformation(
			string? testClassName,
			string? testMethodName);
	}
}
