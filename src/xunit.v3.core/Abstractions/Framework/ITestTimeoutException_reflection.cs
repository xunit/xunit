#pragma warning disable CA1040 // This is intended as a marker interface

using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// This is a marker interface implemented to indicate that the exception is the result
/// of a test timeout, resulting in a failure cause of <see cref="FailureCause.Timeout"/>.
/// </summary>
public interface ITestTimeoutException
{ }
