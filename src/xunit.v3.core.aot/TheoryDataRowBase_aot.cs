namespace Xunit;

partial class TheoryDataRowBase
{
	/// <inheritdoc/>
	public Type? SkipType =>
		throw new PlatformNotSupportedException("SkipType is not used in Native AOT; SkipUnless and SkipWhen already incorporate the type during code generation");

	/// <inheritdoc/>
	public Func<bool>? SkipUnless { get; set; }

	/// <inheritdoc/>
	public Func<bool>? SkipWhen { get; set; }
}
