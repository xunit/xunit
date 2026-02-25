using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// This class is used by the test engine to ensure everything created by the Native AOT generators is
/// registered before the engine attempts to perform any activities.
/// </summary>
/// <param name="initializedAttributes">The attributes that were initialized, and thus need to be
/// cleaned up in disposal.</param>
/// <param name="initException">The exception that happened during initialization, if any.</param>
public sealed class EngineInitialization(
	Stack<EngineInitializationAttribute> initializedAttributes,
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
	/// Starts engine initialization.
	/// </summary>
	/// <param name="assembly">The test assembly</param>
	/// <returns>The <see cref="EngineInitialization"/> object which must be disposed during shutdown</returns>
	public static async ValueTask<EngineInitialization> Start(Assembly? assembly)
	{
		if (assembly is null)
			throw new InvalidOperationException("Assembly.GetEntryAssembly() returned null, which should not be possible");

		var initializedAttributes = new Stack<EngineInitializationAttribute>();

		try
		{
			foreach (var engineInitializationAttribute in assembly.GetCustomAttributes<EngineInitializationAttribute>())
			{
				await engineInitializationAttribute.InitializeAsync();
				initializedAttributes.Push(engineInitializationAttribute);
			}

			return new EngineInitialization(initializedAttributes);
		}
		catch (Exception ex)
		{
			return new EngineInitialization(initializedAttributes, ex);
		}
	}
}
