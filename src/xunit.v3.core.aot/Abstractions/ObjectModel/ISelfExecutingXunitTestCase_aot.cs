#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Reflection-based test execution is not supported in Native AOT
/// </summary>
[Obsolete("Reflection-based test execution is not supported in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ISelfExecutingXunitTestCase
{ }
