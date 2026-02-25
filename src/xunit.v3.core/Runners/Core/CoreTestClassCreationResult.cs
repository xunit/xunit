namespace Xunit.v3;

/// <summary>
/// Represents the result of creating a test class instance.
/// </summary>
public class CoreTestClassCreationResult(object? instance)
{
	/// <summary>
	/// Gets the execution context that was in place during construction.
	/// </summary>
	public ExecutionContext? ExecutionContext { get; } =
		ExecutionContext.Capture();

	/// <summary>
	/// Gets the test class instance.
	/// </summary>
	/// <remarks>
	/// This value will be <see langword="null"/> when the test class is static.
	/// </remarks>
	public object? Instance { get; } = Guard.ArgumentNotNull(instance);

	/// <summary>
	/// Gets the synchronization context that was in place during construction.
	/// </summary>
	public SynchronizationContext? SynchronizationContext { get; } =
		SynchronizationContext.Current;
}
