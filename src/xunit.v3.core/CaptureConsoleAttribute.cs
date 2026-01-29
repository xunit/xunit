#pragma warning disable CA1812  // CaptureConsoleImpl is instantiated dynamically as an assembly fixture
#pragma warning disable CA1822  // The attribute properties cannot be static, because that's not how attribute properties are supposed to work

using Xunit.v3;

namespace Xunit;

/// <summary>
/// Captures <see cref="Console"/> output (<see cref="Console.Out"/> and/or <see cref="Console.Error"/>)
/// and reports it to the test output helper.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class CaptureConsoleAttribute : Attribute, IAssemblyFixtureAttribute
{
	static bool captureError = true;
	static bool captureOut = true;

	Type IAssemblyFixtureAttribute.AssemblyFixtureType =>
		typeof(CaptureConsoleImpl);

	/// <summary>
	/// Gets or sets a flag to indicate whether to override <see cref="Console.Error"/>.
	/// </summary>
	public bool CaptureError
	{
		get => captureError;
		set => captureError = value;
	}

	/// <summary>
	/// Gets or sets a flag to indicate whether to override <see cref="Console.Out"/>
	/// (which includes the <c>Write</c> and <c>WriteLine</c> methods on <see cref="Console"/>).
	/// </summary>
	public bool CaptureOut
	{
		get => captureOut;
		set => captureOut = value;
	}

	sealed class CaptureConsoleImpl : IDisposable
	{
		readonly ConsoleCaptureTestOutputWriter writer;

		public CaptureConsoleImpl() =>
			writer = new ConsoleCaptureTestOutputWriter(TestContextAccessor.Instance, captureError, captureOut);

		public void Dispose() =>
			writer.SafeDispose();
	}
}
