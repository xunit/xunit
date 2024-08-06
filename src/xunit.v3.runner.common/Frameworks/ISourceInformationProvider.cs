using System;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a provider which gives source line information for a test case after discovery has
/// completed. This is typically provided by a third party runner (for example, the VSTest plugin provides
/// this via DiaSession from Visual Studio). It's used to supplement test case metadata when the discovery
/// process itself cannot provide source file and line information.
/// </summary>
public interface ISourceInformationProvider : IAsyncDisposable
{
	/// <summary>
	/// Returns the source information for a test case.
	/// </summary>
	/// <param name="testClassName">The test class name, if known</param>
	/// <param name="testMethodName">The test method name, if known</param>
	/// <returns>The source information, with null string and int values when the information is not available.
	/// Note: return value should never be <c>null</c>, only the interior data values inside.</returns>
	SourceInformation GetSourceInformation(
		string? testClassName,
		string? testMethodName);
}
