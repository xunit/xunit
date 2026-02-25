namespace Xunit.Runner.Common;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate the availability of a custom
/// result writer (in Microsoft Testing Platform mode). This will add command line switches
/// for the writer as <c>"--xunit-result-{ID}"</c> and <c>"--xunit-result-{ID}-filename"</c>.
/// The writer type must implement <see cref="IMicrosoftTestingPlatformResultWriter"/>.
/// </summary>
/// <remarks>
/// Result writer registration attributes are only valid at the assembly level.
/// </remarks>
public interface IRegisterMicrosoftTestingPlatformResultWriterAttribute : IRegisterResultWriterAttribute
{ }
