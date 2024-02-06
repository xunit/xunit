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
[DebuggerDisplay("category = {fixtureCategory}, cache count = {fixtureCache.Count}, known type count = {knownTypes.Count}")]
public class FixtureMappingManager : IAsyncDisposable
{
	volatile bool disposed;
	readonly Dictionary<Type, object> fixtureCache = new();
	readonly string fixtureCategory;
	readonly HashSet<Type> knownTypes = new();
	readonly FixtureMappingManager? parentMappingManager;

	/// <summary>
	/// Initializes a new instance of the <see cref="FixtureMappingManager"/> class.
	/// </summary>
	/// <param name="fixtureCategory">The category of fixture (i.e., "Assembly"); used in exception messages</param>
	/// <param name="parentMappingManager">The parent mapping manager (used to resolve constructor arguments)</param>
	public FixtureMappingManager(
		string fixtureCategory,
		FixtureMappingManager? parentMappingManager = null)
	{
		this.fixtureCategory = fixtureCategory;
		this.parentMappingManager = parentMappingManager;
	}

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
	/// Registers a type as a supported type for this level of fixtures. Any request to <see cref="GetFixture(Type)"/>
	/// that isn't for one of the registered types will be forwarded onto the parent mapping manger, if one was provided.
	/// Note that you can register open generic types here, and the system will be able to provide any instance of a
	/// closed generic version of such types.
	/// </summary>
	/// <param name="type">The registered type</param>
	[Obsolete("use initialize instead")]
	public void AddFixtureType(Type? type)
	{
		if (type is not null)
			knownTypes.Add(type);
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (disposed)
			return;

		GC.SuppressFinalize(this);
		disposed = true;

		var aggregator = new ExceptionAggregator();

		var disposeAsyncTasks =
			fixtureCache
				.Values
				.OfType<IAsyncDisposable>()
				.Select(fixture => aggregator.RunAsync(async () =>
				{
					try
					{
						await fixture.DisposeAsync();
					}
					catch (Exception ex)
					{
						throw new TestFixtureCleanupException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in DisposeAsync", fixtureCategory, fixture.GetType().FullName), ex.Unwrap());
					}
				}).AsTask())
				.ToList();

		if (disposeAsyncTasks.Count != 0)
			await Task.WhenAll(disposeAsyncTasks);

		foreach (var fixture in fixtureCache.Values.OfType<IDisposable>())
			aggregator.Run(() =>
			{
				try
				{
					fixture.Dispose();
				}
				catch (Exception ex)
				{
					throw new TestFixtureCleanupException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in Dispose", fixtureCategory, fixture.GetType().FullName), ex.Unwrap());
				}
			});

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
		{
			if (parentMappingManager is null)
				return null;

			return await parentMappingManager.GetFixture(fixtureType);
		}

		// Ensure there is a single public constructor
		var ctors =
			fixtureType
				.GetConstructors()
				.Where(ci => !ci.IsStatic && ci.IsPublic)
				.ToList();

		if (ctors.Count != 1)
			throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' may only define a single public constructor.", fixtureCategory, fixtureType.FullName));

		// Make sure we can accommodate all the constructor arguments from either known types or the parent
		var ctor = ctors[0];
		var parameters = ctor.GetParameters();
		var ctorArgs = new object[parameters.Length];
		var ctorIdx = 0;
		var missingParameters = new List<ParameterInfo>();

		foreach (var parameter in parameters)
		{
			object? arg = null;
			if (parameter.ParameterType == typeof(_IMessageSink))
				arg = TestContext.Current?.DiagnosticMessageSink;
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
			throw new TestClassException(
				string.Format(
					CultureInfo.CurrentCulture,
					"{0} fixture type '{1}' had one or more unresolved constructor arguments: {2}",
					fixtureCategory,
					fixtureType.FullName,
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
			throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in its constructor", fixtureCategory, fixtureType.SafeName()), ex.Unwrap());
		}

		// Do async initialization
		if (result is IAsyncLifetime asyncLifetime)
			try
			{
				await asyncLifetime.InitializeAsync();
			}
			catch (Exception ex)
			{
				throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in InitializeAsync", fixtureCategory, fixtureType.SafeName()), ex.Unwrap());
			}

		return result;
	}

	/// <inheritdoc/>
	public async ValueTask InitializeAsync(params Type[] fixtureTypes)
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
