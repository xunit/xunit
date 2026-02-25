#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Interface-based attributes are not supported in Native AOT; please use
/// <see cref="TestPipelineStartupAttribute"/> instead
/// </summary>
[Obsolete("Interface-based attributes are not supported in Native AOT; please use TestPipelineStartupAttribute instead", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITestPipelineStartupAttribute
{ }
