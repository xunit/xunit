#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Interface-based attributes are not supported in Native AOT; please use
/// <see cref="TestCaseOrdererAttribute"/> instead
/// </summary>
[Obsolete("Interface-based attributes are not supported in Native AOT; please use TestCaseOrdererAttribute instead", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITestCaseOrdererAttribute
{ }
