namespace Xunit.Runner.Common;

/// <summary>
/// Represents generated code that must be run during runner initialization and cleanup.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
public abstract class RunnerInitializationAttribute : Attribute, IAsyncDisposable
{
	/// <summary>
	/// Override to perform cleanup for anything done during <see cref="InitializeAsync"/>.
	/// </summary>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		return default;
	}

	/// <summary>
	/// Override to perform runner startup initialization.
	/// </summary>
	public abstract ValueTask InitializeAsync();
}
