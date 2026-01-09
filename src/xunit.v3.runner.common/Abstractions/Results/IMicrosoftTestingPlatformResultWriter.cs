namespace Xunit.Runner.Common;

/// <summary>
/// Represents a result writer that can run in Microsoft Testing Platform mode, and report
/// results to an output file of the user's choosing.
/// </summary>
public interface IMicrosoftTestingPlatformResultWriter : IResultWriter
{
	/// <summary>
	/// Gets the default file extension used when the user did not provide a report filename.
	/// This should be returned in <c>"ext"</c> form.
	/// </summary>
	/// <remarks>
	/// Note that default file extensions should be descriptive and unique, since they will
	/// be how users will know which result file came from which result writer when there
	/// are multiple writers when the user has not chosen custom filenames.<br />
	/// <br />
	/// Example:
	/// <list type="bullet">
	/// <item>Good = <c>"xunit"</c></item>
	/// <item>Bad = <c>"xml"</c></item>
	/// </list>
	/// </remarks>
	string DefaultFileExtension { get; }

	/// <summary>
	/// Gets the description of the result that's presented to the user when they
	/// ask for command line help. This will be used for the <c>--xunit-result-{id}</c>
	/// help text.
	/// </summary>
	/// <remarks>
	/// Example: <c>"Enable generating xUnit.net (v2+ XML) report"</c>
	/// </remarks>
	string Description { get; }

	/// <summary>
	/// Gets the description of the result file that's presented to the user when they
	/// ask for command line help. This will be used for the <c>--xunit-result-{id}-filename</c>
	/// help text.
	/// </summary>
	/// <remarks>
	/// Example: <c>"The name of the generated xUnit.net (v2+ XML) report"</c>
	/// </remarks>
	string FileNameDescription { get; }
}
