#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Please use <see cref="ICodeGenTestCase"/> in Native AOT
/// </summary>
[Obsolete("Please use ICodeGenTestCase in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IXunitTestCase
{ }
