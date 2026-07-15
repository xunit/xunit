using Xunit.v3;

namespace Xunit;

partial class TestClassAttribute : ITestClassAttribute
{
	/// <inheritdoc/>
	public bool DisableParallelization { get; set; }
}
