#pragma warning disable CA1040 // This is intended as a marker interface

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Marker interfaces are not consumed in Native AOT.
/// </summary>
[Obsolete("Marker interfaces are not consumed in Native AOT")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITestTimeoutException
{ }
