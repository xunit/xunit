using System.Reflection;

namespace Xunit.Runner.Common;

/// <summary>
/// This class is used by runners to ensure everything created by the Native AOT generators is
/// registered before the runner attempts to run anything.
/// </summary>
/// <param name="initializedAttributes">The attributes that were initialized, and thus need to be
/// cleaned up in disposal.</param>
/// <param name="initException">The exception that happened during initialization, if any.</param>
public sealed class RunnerInitialization(
	Stack<RunnerInitializationAttribute> initializedAttributes,
	Exception? initException = null)
		: IAsyncDisposable
{
	/// <summary>
	/// Gets the exception that was thrown during initialization, if any.
	/// </summary>
	public Exception? InitException => initException;

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		while (initializedAttributes.TryPop(out var attribute))
			await attribute.SafeDisposeAsync();
	}

	/// <summary>
	/// Starts runner initialization.
	/// </summary>
	/// <param name="assembly">The test assembly</param>
	/// <returns>The <see cref="RunnerInitialization"/> object which must be disposed during shutdown</returns>
	public static async ValueTask<RunnerInitialization> Start(Assembly? assembly)
	{
		if (assembly is null)
			throw new InvalidOperationException("Assembly.GetEntryAssembly() returned null, which should not be possible");

		var initializedAttributes = new Stack<RunnerInitializationAttribute>();

		try
		{
			foreach (var engineInitializationAttribute in assembly.GetCustomAttributes<RunnerInitializationAttribute>())
			{
				await engineInitializationAttribute.InitializeAsync();
				initializedAttributes.Push(engineInitializationAttribute);
			}

			return new RunnerInitialization(initializedAttributes);
		}
		catch (Exception ex)
		{
			return new RunnerInitialization(initializedAttributes, ex);
		}
	}
}
