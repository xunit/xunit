namespace Xunit.Runner.Common;

/// <summary>
/// Represents generated code that must be run during runner initialization and cleanup.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
public abstract class RunnerInitializationAttribute : Attribute, IAsyncDisposable
{
	/// <summary>
	/// Used to perform any cleanup before the runner exits.
	/// </summary>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		return default;
	}

	/// <summary>
	/// Used to perform any necessary registration.
	/// </summary>
	public virtual ValueTask InitializeAsync() =>
		default;
}
