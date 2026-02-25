namespace Xunit.v3;

/// <summary>
/// Represents generated code that must be run during engine initialization and cleanup.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
public abstract class EngineInitializationAttribute : Attribute, IAsyncLifetime
{
	/// <summary>
	/// Used to perform any cleanup before the engine exits.
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
