#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Interface-based attributes are not supported in Native AOT; please use
/// <see cref="TestClassOrdererAttribute"/> instead
/// </summary>
[Obsolete("Interface-based attributes are not supported in Native AOT; please use TestClassOrdererAttribute instead", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITestClassOrdererAttribute
{ }
