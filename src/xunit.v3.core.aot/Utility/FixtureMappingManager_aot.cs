using System.Diagnostics;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Maps fixture objects using code-generated factories.
/// </summary>
/// <remarks>
/// Open generic fixtures are not supported (meaning, you can register <c>SomeType&lt;int&gt;</c>,
/// but not <c>SomeType&lt;&gt;</c>), since all factories must be code generated ahead of time.
/// </remarks>
/// <param name="fixtureCategory">The category of fixture (i.e., "Assembly"); used in exception messages</param>
/// <param name="fixtureFactories">The factories which create the fixture objects</param>
/// <param name="parentMappingManager">The parent mapping manager (used for <see cref="TryGetFixtureArgument{T}()"/>)</param>
[DebuggerDisplay("category = {fixtureCategory}, cache count = {fixtureCache.Count}, factory count = {FixtureFactories.Count}")]
public partial class FixtureMappingManager(
	string fixtureCategory,
	IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> fixtureFactories,
	FixtureMappingManager? parentMappingManager = null) :
		IAsyncDisposable
{
	readonly string fixtureCategory = Guard.ArgumentNotNull(fixtureCategory);

	// This constructor is here for the testable constructor in the shared source
	FixtureMappingManager(string fixtureCategory) :
		this(fixtureCategory, CodeGenHelper.EmptyFixtureFactories, null)
	{ }

	/// <summary>
	/// Gets the fixture factories.
	/// </summary>
	/// <remarks>
	/// This is overridable primarily for testing purposes.
	/// </remarks>
	protected virtual IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> FixtureFactories { get; } =
		Guard.ArgumentNotNull(fixtureFactories);

	/// <summary>
	/// Initializes the known fixture types, optionally creating the instances ahead of
	/// time.
	/// </summary>
	/// <param name="createInstances">A flag indicating whether to create the instances</param>
	public async ValueTask InitializeAsync(bool createInstances)
	{
		ObjectDisposedException.ThrowIf(disposed, this);

		if (!createInstances)
			return;

		foreach (var kvp in FixtureFactories)
			await TryGetFixture(kvp.Key);
	}

	async ValueTask<(bool Success, object? Result)> TryGetFixture(Type fixtureType)
	{
		// Pull from the cache if present
		if (fixtureCache.TryGetValue(fixtureType, out var result))
			return (true, result);

		// Ensure this is a type that's known to come from this fixture level; otherwise, ask the
		// parent mapping manager to generate the type (or return null if it's not)
		if (!FixtureFactories.TryGetValue(fixtureType, out var factory))
			return
				parentMappingManager is not null
					? await parentMappingManager.TryGetFixture(fixtureType)
					: (false, null);

		// Create the object
		try
		{
			result = await factory(parentMappingManager);
			fixtureCache[fixtureType] = result;
		}
		catch (TestPipelineException)  // Let anything from the factory itself percolate
		{
			throw;
		}
		catch (Exception ex)  // Report anything else as a constructor failure
		{
			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"{0} fixture type '{1}' threw in its constructor",
					fixtureCategory, fixtureType.SafeName()
				),
				ex.Unwrap()
			);
		}

		// Do async initialization
		if (result is IAsyncLifetime asyncLifetime)
			try
			{
				await asyncLifetime.InitializeAsync();
			}
			catch (Exception ex)
			{
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"{0} fixture type '{1}' threw in InitializeAsync",
						fixtureCategory,
						fixtureType.SafeName()
					),
					ex.Unwrap()
				);
			}

		return (true, result);
	}
}
