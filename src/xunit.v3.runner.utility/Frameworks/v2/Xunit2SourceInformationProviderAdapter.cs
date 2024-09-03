namespace Xunit.Runner.v2;

/// <summary>
/// Class used to adapt v3 source information provider into v2 version.
/// </summary>
public static class Xunit2SourceInformationProviderAdapter
{
	/// <summary>
	/// Create a <see cref="Abstractions.ISourceInformationProvider"/> adapter around a <see cref="Common.ISourceInformationProvider"/>
	/// instance.
	/// </summary>
	public static Abstractions.ISourceInformationProvider Adapt(Common.ISourceInformationProvider v3Provider) =>
		new Xunit2SourceInformationProvider(v3Provider);
}
