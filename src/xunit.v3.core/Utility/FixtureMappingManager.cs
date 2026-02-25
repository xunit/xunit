using Xunit.Sdk;

namespace Xunit.v3;

partial class FixtureMappingManager
{
	volatile bool disposed;
	readonly Dictionary<Type, object> fixtureCache = [];

	/// <summary>
	/// FOR TESTING PURPOSES ONLY.
	/// </summary>
	protected FixtureMappingManager(
		string fixtureCategory,
		object[] cachedFixtureValues) :
			this(fixtureCategory)
	{
		foreach (var cachedFixtureValue in Guard.ArgumentNotNull(cachedFixtureValues))
			fixtureCache[cachedFixtureValue.GetType()] = cachedFixtureValue;
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (disposed)
			return;

		GC.SuppressFinalize(this);
		disposed = true;

		var aggregator = new ExceptionAggregator();

		foreach (var obj in fixtureCache.Values)
		{
			if (obj is IAsyncDisposable asyncDisposable)
				await aggregator.RunAsync(async () =>
				{
					try
					{
						await asyncDisposable.DisposeAsync();
					}
					catch (Exception ex)
					{
						throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in DisposeAsync", fixtureCategory, obj.GetType().SafeName()), ex.Unwrap());
					}
				});
			else if (obj is IDisposable disposable)
				aggregator.Run(() =>
				{
					try
					{
						disposable.Dispose();
					}
					catch (Exception ex)
					{
						throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in Dispose", fixtureCategory, obj.GetType().SafeName()), ex.Unwrap());
					}
				});
		}

		aggregator.ThrowIfFaulted();
	}

	/// <summary>
	/// Get a value for the given fixture type. If the fixture type is unknown, then returns <see langword="null"/>.
	/// </summary>
	/// <param name="fixtureType">The type of the fixture</param>
	/// <returns>Returns the value if the fixture type is found, or <see langword="null"/> if it's not.</returns>
	public async ValueTask<object?> GetFixture(Type fixtureType)
	{
		ObjectDisposedException.ThrowIf(disposed, this);

		var fixture = await TryGetFixture(fixtureType);
		return fixture.Result;
	}

	/// <summary>
	/// Get a value for the given fixture type. If the fixture type is unknown, then returns <see langword="default"/>.
	/// </summary>
	/// <typeparam name="T">The type of the fixture</typeparam>
	public async ValueTask<T?> GetFixture<T>() =>
		(await TryGetFixture<T>()).Result;

	/// <summary>
	/// Tries to get a strongly typed fixture value.
	/// </summary>
	/// <typeparam name="T">The fixture type</typeparam>
	/// <returns>The result with a flag which indicates whether it was successful</returns>
	/// <remarks>
	/// This only returns fixture values, which differs from <see cref="TryGetFixtureArgument{T}()"/>
	/// which will also return values for supported non-fixture argument types (like
	/// <see cref="ITestContextAccessor"/> or <see cref="ITestOutputHelper"/>).
	/// </remarks>
	public async ValueTask<(bool Success, T? Result)> TryGetFixture<T>()
	{
		ObjectDisposedException.ThrowIf(disposed, this);

		var fixture = await TryGetFixture(typeof(T));
		if (fixture.Success && fixture.Result is T resultAsT)
			return (true, resultAsT);

		return (false, default);
	}

	/// <summary>
	/// Tries to get a fixture argument to help construct a fixture. The potential fixed argument
	/// types are supported (e.g., <see cref="IMessageSink"/> and <see cref="ITestContextAccessor"/>),
	/// as well as consulting the parent mapping manager.
	/// </summary>
	/// <typeparam name="T">The fixture argument type to supply</typeparam>
	/// <remarks>
	/// This differs from <see cref="TryGetFixture{T}()"/>, which only returns fixture values and not
	/// additional supported argument types.
	/// </remarks>
	public ValueTask<(bool Success, T? Result)> TryGetFixtureArgument<T>()
	{
		ObjectDisposedException.ThrowIf(disposed, this);

		return TryGetFixtureArgument<T>(this);
	}

	/// <summary>
	/// Tries to get a fixture argument to help construct a fixture. The potential fixed argument
	/// types are supported (e.g., <see cref="IMessageSink"/>, <see cref="ITestContextAccessor"/>,
	/// and <see cref="ITestOutputHelper"/>), as well as consulting the parent mapping manager.
	/// </summary>
	/// <param name="mappingManager">The fixture mapping manager to get fixture instances from</param>
	/// <typeparam name="T">The fixture argument type to supply</typeparam>
	/// <remarks>
	/// This is typically only used in Native AOT, since the current lifetime management for reflection-based
	/// testing would require a delayed retrieval of <see cref="ITestOutputHelper"/>, whereas the code generation-based
	/// runners delay resolution until just as the test is about to run.
	/// </remarks>
	public static async ValueTask<(bool Success, T? Result)> TryGetFixtureArgument<T>(FixtureMappingManager? mappingManager)
	{
		if (typeof(T) == typeof(IMessageSink))
			return (true, (T)(TestContext.CurrentInternal.DiagnosticMessageSink ?? NullMessageSink.Instance));

		if (typeof(T) == typeof(ITestContextAccessor))
			return (true, (T)(ITestContextAccessor)TestContextAccessor.Instance);

		if (typeof(T) == typeof(ITestOutputHelper))
			return (true, (T)TestContext.Current.TestOutputHelper!);

		if (mappingManager is not null)
			return await mappingManager.TryGetFixture<T>();

		return (false, default);
	}
}
