#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.Runner.Common;

/// <summary>
/// Interface-based attributes are not supported in Native AOT; please use
/// <see cref="RegisterResultWriterAttribute"/> instead
/// </summary>
[Obsolete("Interface-based attributes are not supported in Native AOT; please use RegisterResultWriterAttribute instead", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IRegisterResultWriterAttribute
{ }
