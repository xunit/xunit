using System;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// An exception type that is used to <see cref="TestPlatformExecutionMessageSink"/> to report failed
/// test exception information back to Microsoft.Testing.Platform.
/// </summary>
/// <param name="metadata"></param>
/// <remarks>
/// This class is an implementation detail for Microsoft.Testing.Platform that is public for testing purposes.
/// Use this class at your own risk, as breaking changes may occur as needed.
/// </remarks>
[Serializable]
public sealed class XunitException(IErrorMetadata metadata) :
	Exception(ExceptionUtility.CombineMessages(metadata))
{
	/// <inheritdoc/>
	public override string? StackTrace { get; } = ExceptionUtility.CombineStackTraces(metadata);
}
