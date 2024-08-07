using System;
using Xunit.Sdk;

namespace Xunit.Runner.InProc.SystemConsole.TestingPlatform;

/// <summary>
/// An exception type that is used to <see cref="TestPlatformExecutionMessageSink"/> to report failed
/// test exception information back to Microsoft.Testing.Platform.
/// </summary>
/// <param name="metadata"></param>
[Serializable]
public sealed class XunitException(IErrorMetadata metadata) :
	Exception(ExceptionUtility.CombineMessages(metadata))
{
	/// <inheritdoc/>
	public override string? StackTrace { get; } = ExceptionUtility.CombineStackTraces(metadata);
}
