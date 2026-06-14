#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Interface-based attributes are not supported in Native AOT; please use
/// <see cref="TestClassAttribute"/> instead
/// </summary>
[Obsolete("Interface-based attributes are not supported in Native AOT; please use TestClassAttribute instead", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITestClassAttribute
{ }
