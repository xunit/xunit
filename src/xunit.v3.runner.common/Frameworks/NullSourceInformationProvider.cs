using System.Threading.Tasks;

namespace Xunit.Runner.Common;

/// <summary>
/// A null implementation of <see cref="ISourceInformationProvider"/> which always returns empty
/// source information. Get the singleton via <see cref="Instance"/>.
/// </summary>
public sealed class NullSourceInformationProvider : ISourceInformationProvider
{
	NullSourceInformationProvider() { }

	/// <summary>
	/// Gets the singleton instance of the <see cref="NullSourceInformationProvider"/>.
	/// </summary>
	public static NullSourceInformationProvider Instance { get; } = new NullSourceInformationProvider();

	/// <inheritdoc/>
	public ValueTask DisposeAsync() =>
		default;

	/// <inheritdoc/>
	public SourceInformation GetSourceInformation(
		string? testClassName,
		string? testMethodName) =>
			SourceInformation.Null;
}
