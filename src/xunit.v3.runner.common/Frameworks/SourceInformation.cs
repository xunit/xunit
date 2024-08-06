namespace Xunit.Runner.Common;

/// <summary>
/// Source information returned by <see cref="ISourceInformationProvider"/>.
/// </summary>
/// <param name="sourceFile">The source file, if known</param>
/// <param name="sourceLine">The line number, if known</param>
public readonly struct SourceInformation(string? sourceFile, int? sourceLine)
{
	/// <summary>
	/// Gets the source file, if known; <c>null</c>, otherwise
	/// </summary>
	public string? SourceFile { get; } = sourceFile;

	/// <summary>
	/// Gets the source line number, if known; <c>null</c>, otherwise
	/// </summary>
	public int? SourceLine { get; } = sourceLine;
}
