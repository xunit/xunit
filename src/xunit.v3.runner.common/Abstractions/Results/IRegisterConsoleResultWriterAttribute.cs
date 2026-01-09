namespace Xunit.Runner.Common;

/// <summary>
/// Used to decorate xUnit.net test assemblies to indicate the availability of a custom
/// result writer (in console mode). This will add a command line switch for the writer
/// as <c>"-result-{ID}"</c>. The writer type must implement <see cref="IConsoleResultWriter"/>.
/// </summary>
/// <remarks>
/// Result writer registration attributes are only valid at the assembly level.
/// </remarks>
public interface IRegisterConsoleResultWriterAttribute : IRegisterResultWriterAttribute
{ }
