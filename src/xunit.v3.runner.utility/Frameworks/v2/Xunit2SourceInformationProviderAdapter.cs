using Xunit.Abstractions;
using Xunit.Runner.Common;

namespace Xunit.Runner.v2;

/// <summary>
/// Class used to adapt v3 source information provider into v2 version.
/// </summary>
public static class Xunit2SourceInformationProviderAdapter
{
	/// <summary>
	/// Create a <see cref="ISourceInformationProvider"/> adapter around a <see cref="_ISourceInformationProvider"/>
	/// instance.
	/// </summary>
	public static ISourceInformationProvider Adapt(_ISourceInformationProvider v3Provider) =>
		new Xunit2SourceInformationProvider(v3Provider);
}
