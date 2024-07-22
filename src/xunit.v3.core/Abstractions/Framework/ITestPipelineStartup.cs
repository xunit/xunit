using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Interface implemented by developers who want to run code during test pipeline startup and shutdown.
/// A single instance of this may be decorated with an instance of <see cref="ITestPipelineStartupAttribute"/>
/// (typically <see cref="TestPipelineStartupAttribute"/>) at the assembly level.
/// </summary>
/// <remarks>
/// Unlike assembly-level fixtures, this code runs for both discovery and execution (whereas fixtures only
/// run during execution), and it occurs at a much earlier point in the pipeline. The intention with this
/// hook is primarily about ensuring that some essential infrastructure is in place before test discovery
/// takes place. Activities which are only used during execution should be done with assembly-level fixtures.
/// </remarks>
public interface ITestPipelineStartup
{
	/// <summary>
	/// Indicates that the test assembly is starting up.
	/// </summary>
	/// <param name="diagnosticMessageSink">A message sink to which it can report <see cref="IDiagnosticMessage"/>
	/// instances.</param>
	ValueTask StartAsync(IMessageSink diagnosticMessageSink);

	/// <summary>
	/// Inidicates that the test assembly is shutting down.
	/// </summary>
	ValueTask StopAsync();
}
