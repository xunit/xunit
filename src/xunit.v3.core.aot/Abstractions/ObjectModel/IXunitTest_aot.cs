#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Please use <see cref="ICodeGenTest"/> in Native AOT
/// </summary>
[Obsolete("Please use ICodeGenTest in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IXunitTest
{ }
