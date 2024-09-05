using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Maps fixture objects, including support for generic collection fixtures.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FixtureMappingManager"/> class.
/// </remarks>
/// <param name="fixtureCategory">The category of fixture (i.e., "Assembly"); used in exception messages</param>
/// <param name="parentMappingManager">The parent mapping manager (used to resolve constructor arguments)</param>
[DebuggerDisplay("category = {fixtureCategory}, cache count = {fixtureCache.Count}, known type count = {knownTypes.Count}")]
public class FixtureMappingManager(
	string fixtureCategory,
	FixtureMappingManager? parentMappingManager = null) :
		IAsyncDisposable
{
	volatile bool disposed;
	readonly Dictionary<Type, object> fixtureCache = [];
	readonly string fixtureCategory = fixtureCategory;
	readonly HashSet<Type> knownTypes = [];
#pragma warning disable CA2213  // We don't own the lifetime of this, so we shouldn't dispose of it
	readonly FixtureMappingManager? parentMappingManager = parentMappingManager;
#pragma warning restore CA2213

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

	internal IReadOnlyDictionary<Type, object> FixtureCache => fixtureCache;

	/// <summary>
	/// Returns a list of the known fixture types at this category level. This will not include fixture
	/// types known from parent categories and above.
	/// </summary>
	public IReadOnlyCollection<Type> LocalFixtureTypes => knownTypes;

	/// <summary>
	/// Returns a list of all known fixture types at all category levels.
	/// </summary>
	public IReadOnlyCollection<Type> GlobalFixtureTypes => fixtureCache.Keys;

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
	/// Get a value for the given fixture type. If the fixture type is unknown, then returns <c>null</c>.
	/// </summary>
	/// <param name="fixtureType">The type of the fixture</param>
	/// <returns>Returns the value if the fixture type is found, or <c>null</c> if it's not.</returns>
	public async ValueTask<object?> GetFixture(Type fixtureType)
	{
		if (disposed)
			throw new ObjectDisposedException(nameof(FixtureMappingManager));

		Guard.ArgumentNotNull(fixtureType);

		// Pull from the cache if present
		if (fixtureCache.TryGetValue(fixtureType, out var result))
			return result;

		// Ensure this is a type that's known to come from this fixture level; otherwise, ask the
		// parent mapping manager to generate the type (or return null if it's not)
		var isKnownType = knownTypes.Contains(fixtureType);
		if (!isKnownType && fixtureType.IsGenericType)
			isKnownType = knownTypes.Contains(fixtureType.GetGenericTypeDefinition());

		if (!isKnownType)
			return
				parentMappingManager is not null
					? await parentMappingManager.GetFixture(fixtureType)
					: null;

		// Ensure there is a single public constructor
		var ctors =
			fixtureType
				.GetConstructors()
				.Where(ci => !ci.IsStatic && ci.IsPublic)
				.ToList();

		if (ctors.Count != 1)
			throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' may only define a single public constructor.", fixtureCategory, fixtureType.SafeName()));

		// Make sure we can accommodate all the constructor arguments from either known types or the parent
		var ctor = ctors[0];
		var parameters = ctor.GetParameters();
		var ctorArgs = new object[parameters.Length];
		var ctorIdx = 0;
		var missingParameters = new List<ParameterInfo>();

		foreach (var parameter in parameters)
		{
			object? arg = null;
			if (parameter.ParameterType == typeof(IMessageSink))
				arg = TestContext.CurrentInternal.DiagnosticMessageSink ?? NullMessageSink.Instance;
			else if (parameter.ParameterType == typeof(ITestContextAccessor))
				arg = TestContextAccessor.Instance;
			else if (parentMappingManager is not null)
				arg = await parentMappingManager.GetFixture(parameter.ParameterType);

			if (arg is null)
				missingParameters.Add(parameter);
			else
				ctorArgs[ctorIdx++] = arg;
		}

		if (missingParameters.Count > 0)
			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"{0} fixture type '{1}' had one or more unresolved constructor arguments: {2}",
					fixtureCategory,
					fixtureType.SafeName(),
					string.Join(", ", missingParameters.Select(p => string.Format(CultureInfo.CurrentCulture, "{0} {1}", p.ParameterType.Name, p.Name)))
				)
			);

		// Create the object
		try
		{
			result = ctor.Invoke(ctorArgs);
			fixtureCache[fixtureType] = result;
		}
		catch (Exception ex)
		{
			throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in its constructor", fixtureCategory, fixtureType.SafeName()), ex.Unwrap());
		}

		// Do async initialization
		if (result is IAsyncLifetime asyncLifetime)
			try
			{
				await asyncLifetime.InitializeAsync();
			}
			catch (Exception ex)
			{
				throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in InitializeAsync", fixtureCategory, fixtureType.SafeName()), ex.Unwrap());
			}

		return result;
	}

	/// <inheritdoc/>
	public ValueTask InitializeAsync(params Type[] fixtureTypes) =>
		InitializeAsync((IReadOnlyCollection<Type>)fixtureTypes);

	/// <inheritdoc/>
	public async ValueTask InitializeAsync(IReadOnlyCollection<Type> fixtureTypes)
	{
		Guard.ArgumentNotNull(fixtureTypes);

		foreach (var fixtureType in fixtureTypes)
		{
			var knownType = fixtureType;

			// If this looks like FixtureType<T> instead of FixtureType<int>, we want the known type to be
			// the open generic (i.e. FixtureType<>), which also means we can't directly create it now.
			// We may be asked to create it later as a dependency, at which point we'll know the concrete
			// type for the dependency.
			if (fixtureType.IsGenericType && fixtureType.GenericTypeArguments.Any(t => t.IsGenericParameter))
				knownType = fixtureType.GetGenericTypeDefinition();

			knownTypes.Add(knownType);

			// Pre-create the fixture type, because we want to make sure all concrete fixtures are
			// instantiated even if nobody comes along later to get the instance.
			if (knownType == fixtureType)
				await GetFixture(knownType);
		}
	}
}
