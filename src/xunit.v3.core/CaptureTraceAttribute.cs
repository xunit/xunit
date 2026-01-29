#pragma warning disable CA1812  // CaptureTraceImpl is instantiated dynamically as an assembly fixture

using System.Diagnostics;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Captures <see cref="Trace"/> and <see cref="Debug"/> output and reports it to the
/// test output helper.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class CaptureTraceAttribute : Attribute, IAssemblyFixtureAttribute
{
	Type IAssemblyFixtureAttribute.AssemblyFixtureType =>
		typeof(CaptureTraceImpl);

	sealed class CaptureTraceImpl : IDisposable
	{
		readonly TraceCaptureTestOutputWriter writer;

		public CaptureTraceImpl() =>
			writer = new TraceCaptureTestOutputWriter(TestContextAccessor.Instance);

		public void Dispose() =>
			writer.SafeDispose();
	}
}
